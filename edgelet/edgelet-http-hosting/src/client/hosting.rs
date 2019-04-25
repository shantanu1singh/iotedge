// Copyright (c) Microsoft. All rights reserved.

use std::sync::Arc;

use failure::{Fail, ResultExt};
use futures::prelude::*;
use hyper::{Client};
use hosting::apis::client::APIClient;
use hosting::apis::configuration::Configuration;
use hosting::models::*;
use hosting::apis::HostingApi;
use url::Url;

use edgelet_core::{UrlExt};
use edgelet_http::{UrlConnector};

use crate::error::{Error, ErrorKind};

pub trait HostingInterface {
    type Error: Fail;

    type DeviceConnectionInformationFuture: Future<Item = DeviceConnectionInfo, Error = Self::Error>
    + Send;

    fn get_device_connection_information(&self) -> Self::DeviceConnectionInformationFuture;
}

pub struct HostingClient<'a>
{
    api_client: Arc<APIClient>,
    client: &'a Box<dyn HostingApi>,
}

impl HostingClient
{
    pub fn new(url: &Url) -> Result<Self, Error> {
        let client = Client::builder()
            .build(UrlConnector::new(url).context(ErrorKind::InitializeHostingClient)?);

        let base_path = url
            .to_base_path()
            .context(ErrorKind::InitializeHostingClient)?;
        let mut configuration = Configuration::new(client);
        configuration.base_path = base_path
            .to_str()
            .ok_or(ErrorKind::InitializeHostingClient)?
            .to_string();

        let scheme = url.scheme().to_string();
        configuration.uri_composer = Box::new(move |base_path, path| {
            Ok(UrlConnector::build_hyper_uri(&scheme, base_path, path)?)
        });

        let api_client_instance = Arc::new(APIClient::new(configuration));
        let hosting_client = HostingClient {
            api_client: api_client_instance,
            client: api_client_instance.hosting_api()
        };

        Ok(hosting_client)
    }
}

impl Clone for HostingClient
{
    fn clone(&self) -> Self {
        let clone = self.api_client.clone();
        HostingClient {
            api_client: clone,
            client: clone.hosting_api()
        }
    }
}

impl HostingInterface for HostingClient
{
    type Error = Error;

    type DeviceConnectionInformationFuture =
    Box<dyn Future<Item = DeviceConnectionInfo, Error = Self::Error> + Send>;

    fn get_device_connection_information(&self) -> Self::DeviceConnectionInformationFuture {
        let connection_info = self
            .client
            .get_device_connection_information(crate::HOSTING_API_VERSION)
            .map_err(|err| {
                Error::from_hosting_error(
                    err,
                    ErrorKind::GetDeviceConnectionInformation,
                )
            });
        Box::new(connection_info)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use hosting::apis::Error as HostingError;
    use hosting::apis::ApiError as HostingApiError;
//    use hosting::models::*;

    #[test]
    fn invalid_hosting_url() {
        let client =
            HostingClient::new(&(Url::parse("fd://").unwrap()));
        match client {
            Ok(_t) => { panic!("Unexpected to succeed with invalid Url.")}
            Err( ref err) => {
                match err.kind() {
                    ErrorKind::InitializeHostingClient => {}
                    _ => {
                        panic!("Expected `InitializeHostingClient` but got {:?}", err);
                    }
                }
            }
        };
    }

    #[test]
    fn valid_hosting_url() {
        let client =
            HostingClient::new(&(Url::parse("http://localhost:99/").unwrap()));
        assert!(client.is_ok());
    }

    struct TestHostingApi<T>
        where T: Send + Sync
    {
        pub error : Option<HostingError<T>>,
    }

    impl<T> HostingApi for TestHostingApi<T>
        where T: Send + Sync
    {
        fn get_device_connection_information(
            &self,
            api_version: &str,
        ) -> Box<dyn Future<Item = hosting::models::DeviceConnectionInfo, Error = HostingError<serde_json::Value>> + Send>{
            match self.error.as_ref() {
                None => Box::new(Ok(DeviceConnectionInfo::new(
                    "hub".to_string(), "device".to_string())).into_future()),
                Some(s) => Box::new(Err(s.clone()).into_future())
            }
        }

        fn sign(
            &self,
            api_version: &str,
            payload: hosting::models::SignRequest,
        ) -> Box<dyn Future<Item = hosting::models::SignResponse, Error = HostingError<serde_json::Value>> + Send>{
            match self.error.as_ref() {
                None => Box::new(Ok(SignResponse::new("a".to_string())).into_future()),
                Some(s) => Box::new(Err(s.clone()).into_future())
            }
        }
    }

    #[test]
    fn get_device_connection_info_error() {
        // arrange
        let hosting_error = HostingError::Hyper(None);
        let hosting_api = TestHostingApi { error: Some(hosting_error) };
        let client = HostingClient{
            api_client: None,
            client: hosting_api
        };

        let execute = |client: HostingClient| {
            client.get_device_connection_information(crate::HOSTING_API_VERSION)
                .map(|| panic!("Expected a failure."))
                .map_err(|err| {
                    match err.kind() {
                        ErrorKind::GetDeviceConnectionInformation => {
                            match err.cause() {
                                None => {}
                                Some(_t) =>  {}
                            }

//                            assert_eq!(client.error, err.context().);
                        }
                        _ => {
                            panic!("Expected `GetDeviceConnectionInformation` but got {:?}", err);
                        }
                    }
                });
        };

        execute(client);

        let hosting_error = HostingError::Serde(None);
        let client = TestHostingApi { error: Some(hosting_error) };
        execute(client);

        let hosting_error = HostingError::ApiError(HostingApiError { code: hyper::StatusCode::from_u16(400).unwrap(), content: None });
        let client = TestHostingApi { error: Some(hosting_error) };
        execute(client);
    }

    #[test]
    fn get_device_connection_info_success() {

    }
}
