// Copyright (c) Microsoft. All rights reserved.

//use std::fmt;
//use std::str::FromStr;
use std::sync::Arc;
//use std::time::Duration;

use bytes::Bytes;
//use chrono::{DateTime, Utc};
use failure::{Fail, ResultExt};
//use futures::future::{self, FutureResult};
use futures::prelude::*;
//use futures::stream;
use hyper::{Client};
use hosting::apis::client::APIClient;
use hosting::apis::configuration::Configuration;
use hosting::models::*;
//use serde_json;
use url::Url;

//use edgelet_core::*;
use edgelet_core::{UrlExt};
use edgelet_http::{UrlConnector};

use crate::error::{Error, ErrorKind};
use edgelet_core::crypto::Signature;

pub trait HostingInterface {
    type Error: Fail;

    //    type ModuleRegistry: ModuleRegistry<Config = Self::Config, Error = Self::Error>;
    //    type Chunk: AsRef<[u8]>;

    type DeviceConnectionInformationFuture: Future<Item = DeviceConnectionInfo, Error = Self::Error>
    + Send;

    type SignFuture: Future<Item = Option<Bytes>, Error = Self::Error> + Send;
    //    type Logs: Stream<Item = Self::Chunk, Error = Self::Error> + Send;
    //
    //    type CreateFuture: Future<Item = (), Error = Self::Error> + Send;
    //    type GetFuture: Future<Item = (Self::Module, ModuleRuntimeState), Error = Self::Error> + Send;
    //
    //    type ListFuture: Future<Item = Vec<Self::Module>, Error = Self::Error> + Send;
    //    type ListWithDetailsStream: Stream<
    //        Item = (Self::Module, ModuleRuntimeState),
    //        Error = Self::Error,
    //    > + Send;
    //    type LogsFuture: Future<Item = Self::Logs, Error = Self::Error> + Send;
    //    type RemoveFuture: Future<Item = (), Error = Self::Error> + Send;
    //    type RestartFuture: Future<Item = (), Error = Self::Error> + Send;
    //    type StartFuture: Future<Item = (), Error = Self::Error> + Send;
    //    type StopFuture: Future<Item = (), Error = Self::Error> + Send;
    //    type SystemInfoFuture: Future<Item = SystemInfo, Error = Self::Error> + Send;
    //    type RemoveAllFuture: Future<Item = (), Error = Self::Error> + Send;
    //    type TopFuture: Future<Item = ModuleTop, Error = Self::Error> + Send;

    fn get_device_connection_information(&self) -> Self::DeviceConnectionInformationFuture;

    fn sign(&self, identity: Option<&[u8]>, signing_algorithm: &str, data: &[u8]) -> Self::SignFuture;
    //    fn create(&self, module: ModuleSpec<Self::Config>) -> Self::CreateFuture;
    //    fn get(&self, id: &str) -> Self::GetFuture;
    //    fn start(&self, id: &str) -> Self::StartFuture;
    //    fn stop(&self, id: &str, wait_before_kill: Option<Duration>) -> Self::StopFuture;
    //    fn restart(&self, id: &str) -> Self::RestartFuture;
    //    fn remove(&self, id: &str) -> Self::RemoveFuture;
    //    fn system_info(&self) -> Self::SystemInfoFuture;
    //    fn list(&self) -> Self::ListFuture;
    //    fn list_with_details(&self) -> Self::ListWithDetailsStream;
    //    fn logs(&self, id: &str, options: &LogOptions) -> Self::LogsFuture;
    //    fn registry(&self) -> &Self::ModuleRegistry;
    //    fn remove_all(&self) -> Self::RemoveAllFuture;
    //    fn top(&self, id: &str) -> Self::TopFuture;
}

//#[derive(Debug)]
//struct HostingTokenSource;
//
//impl TokenSource for HostingTokenSource {
//    type Error = Error;
//    fn get(&self, _expiry: &DateTime<Utc>) -> Result<String, Error> {
//        Ok("token".to_string())
//    }
//}
//
//impl Clone for HostingTokenSource {
//    fn clone(&self) -> Self {
//        HostingTokenSource {}
//    }
//}

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

    //    type ModuleRegistry: ModuleRegistry<Config = Self::Config, Error = Self::Error>;
    //    type Chunk: AsRef<[u8]>;

    type DeviceConnectionInformationFuture =
    Box<dyn Future<Item = DeviceConnectionInfo, Error = Self::Error> + Send>;

    type SignFuture = Box<dyn Future<Item = Option<Bytes>, Error = Self::Error> + Send>;

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

    fn sign(&self, identity: Option<&[u8]>, signing_algorithm: &str, data: &[u8]) -> Self::SignFuture {
        let mut sign_request = SignRequest::new(signing_algorithm.to_string(), base64::encode(&data));

        match identity {
            None => {},
            Some(s) => {
                sign_request.set_identity(base64::encode(s));
            }
        };

        println!("{:?}", sign_request);

        let sign_response = self
            .client
            .hosting_api()
            .sign(crate::HOSTING_API_VERSION, sign_request)
            .map_err(|err| {
                Error::from_hosting_error(
                    err,
                    ErrorKind::SignData,
                )
            })
            .and_then(|sign_response| {
                base64::decode(sign_response.digest())
                    .and_then(|res|{
                        Ok(Some(Bytes::from(res.as_bytes())))
                    })
                    .map_err(|_e| Error::from(ErrorKind::MalformedResponse))
            });
        Box::new(sign_response)
    }
}
