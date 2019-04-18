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
