// Copyright (c) Microsoft. All rights reserved.

use std::sync::Arc;

use failure::{Fail, ResultExt};
use futures::prelude::*;
use hyper::{Client};
use hosting::apis::client::APIClient;
use hosting::apis::configuration::Configuration;
use hosting::models::*;
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

pub struct HostingClient {
    client: Arc<APIClient>,
}

impl HostingClient {
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

        let hosting_client = HostingClient {
            client: Arc::new(APIClient::new(configuration)),
        };
        Ok(hosting_client)
    }
}

impl Clone for HostingClient {
    fn clone(&self) -> Self {
        HostingClient {
            client: self.client.clone(),
        }
    }
}

impl HostingInterface for HostingClient {
    type Error = Error;

    type DeviceConnectionInformationFuture =
    Box<dyn Future<Item = DeviceConnectionInfo, Error = Self::Error> + Send>;

    fn get_device_connection_information(&self) -> Self::DeviceConnectionInformationFuture {
        let connection_info = self
            .client
            .hosting_api()
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

//    #[test]
//    fn get_device_connection_info_error() {
//        // arrange
//        let client = HostingClient { client: new A};
//        assert!(client.is_err());
//
//        let config = TestConfig::new("microsoft/test-image".to_string());
//        let module: TestModule<Error> =
//            TestModule::new("test-module".to_string(), config, Ok(state));
//        let runtime = TestRuntime::new(Ok(module));
//        let handler = GetSystemInfo::new(runtime);
//        let request = Request::get("http://localhost/info")
//            .body(Body::default())
//            .unwrap();
//
//        // act
//        let response = handler.handle(request, Parameters::new()).wait().unwrap();
//
//        // assert
//        response
//            .into_body()
//            .concat2()
//            .and_then(|b| {
//                let system_info: SystemInfo = serde_json::from_slice(&b).unwrap();
//                let os_type = system_info.os_type();
//                let architecture = system_info.architecture();
//
//                assert_eq!("os_type_sample", os_type);
//                assert_eq!("architecture_sample", architecture);
//                assert_eq!(edgelet_core::version(), system_info.version());
//
//                Ok(())
//            })
//            .wait()
//            .unwrap();
//    }
//
//    #[test]
//    fn get_device_connection_info_success() {
//
//    }
}
