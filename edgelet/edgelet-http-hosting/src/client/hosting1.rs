// Copyright (c) Microsoft. All rights reserved.

//use std::fmt;
//use std::str::FromStr;
use std::sync::{Arc, RwLock};

use chrono::{DateTime, Utc};
use failure::{Fail, ResultExt};
//use futures::future::{self, FutureResult};
use futures::prelude::*;
//use futures::stream;
use hyper::Method; //, StatusCode, Body, Chunk as HyperChunk, Client
use log::debug;
//use serde_json;
use url::Url;

//use edgelet_core::*;
//use edgelet_core::{ModuleOperation, RuntimeOperation, SystemInfo as CoreSystemInfo, UrlExt};
//use edgelet_http::{UrlConnector};
use edgelet_http::client::{Client as HttpClient, ClientImpl, TokenSource};
//use edgelet_http::ErrorKind as HttpErrorKind;

use crate::error::{Error, ErrorKind};
use crate::models::*;

pub trait HostingInterface {
    type Error: Fail;

    //    type ModuleRegistry: ModuleRegistry<Config = Self::Config, Error = Self::Error>;
    //    type Chunk: AsRef<[u8]>;

    type DeviceConnectionInformationFuture: Future<Item = DeviceConnectionInfo, Error = Self::Error>
        + Send;
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

#[derive(Debug)]
struct HostingTokenSource;

impl TokenSource for HostingTokenSource {
    type Error = Error;
    fn get(&self, _expiry: &DateTime<Utc>) -> Result<String, Error> {
        Ok("token".to_string())
    }
}

impl Clone for HostingTokenSource {
    fn clone(&self) -> Self {
        HostingTokenSource {}
    }
}

#[derive(Clone)]
pub struct HostingClient<C>
where
    C: ClientImpl,
{
    client: Arc<RwLock<HttpClient<C, HostingTokenSource>>>,
}

impl<C> HostingClient<C>
where
    C: 'static + ClientImpl,
{
    pub fn new(
        client_impl: C,
        hosting_environment_endpoint: Url,
        api_version: String,
    ) -> Result<Self, Error> {
        //        let endpoint = UrlConnector::build_hyper_uri(
        //            hosting_environment_endpoint.scheme(),
        //        hosting_environment_endpoint.path(),
        //        "").context(ErrorKind::HostingClientInitializationFailure)?;

        let http_client = HttpClient::new(
            client_impl,
            Some(HostingTokenSource),
            api_version,
            hosting_environment_endpoint,
        )
        .context(ErrorKind::HostingClientInitializationFailure)?;
        Ok(HostingClient {
            client: Arc::new(RwLock::new(http_client)),
        })
    }
}

impl<C> HostingInterface for HostingClient<C>
where
    C: 'static + ClientImpl,
{
    type Error = Error;

    //    type ModuleRegistry: ModuleRegistry<Config = Self::Config, Error = Self::Error>;
    //    type Chunk: AsRef<[u8]>;

    type DeviceConnectionInformationFuture =
        Box<dyn Future<Item = DeviceConnectionInfo, Error = Self::Error> + Send>;

    //    type SignFuture = Box<dyn Future<Item = DeviceConnectionInfo, Error = Self::Error> + Send>;
    //
    //    type DeviceConnectionInformationFuture = Box<dyn Future<Item = DeviceConnectionInfo, Error = Self::Error> + Send>;
    //    fn get_device_connection_information(&self) -> Self::SystemInfoFuture {
    //        unimplemented!()
    //    }

    fn get_device_connection_information(&self) -> Self::DeviceConnectionInformationFuture {
        //        let result = self
        //            .client
        //            .()
        //            .module_logs(
        //                "".to_string(),
        //                &id,
        //                options.follow(),
        //                tail,
        //                options.since(),
        //            )
        //            .then(|logs| match logs {
        //                Ok(logs) => Ok(Logs(id, logs)),
        //                Err(err) => Err(Error::from_mgmt_error(
        //                    err,
        //                    ErrorKind::RuntimeOperation(RuntimeOperation::GetModuleLogs(id)),
        //                )),
        //            });
        //        Box::new(result)

        let request = self
            .client
            .read()
            .expect("RwLock read failure")
            .clone()
            //            .with_token_source(token_source)
            .request::<(), DeviceConnectionInfo>(
                Method::GET,
                "/edge/device/connectioninformation",
                None,
                None,
                false,
            )
            .map_err(|err| Error::from(err.context(ErrorKind::GetDeviceConnectionInformation)))
            .and_then(|device_connection_info| {
                debug!("{:?}", device_connection_info);
                //                    device_connection_info?

                device_connection_info
                    .ok_or_else(|| Error::from(ErrorKind::GetDeviceConnectionInformation))
            });
        Box::new(request)
    }
}
