// Copyright (c) Microsoft. All rights reserved.

use std::fmt;
use std::str::FromStr;
use std::sync::{Arc, RwLock};
use std::time::Duration;

use failure::{Fail, ResultExt};
use futures::future::{self, FutureResult};
use futures::prelude::*;
use futures::stream;
use hyper::{Body, Chunk as HyperChunk, Client};
use serde_json;
use url::Url;

use edgelet_core::*;
use edgelet_core::{ModuleOperation, RuntimeOperation, SystemInfo as CoreSystemInfo, UrlExt};
use edgelet_http::{UrlConnector, API_VERSION};
use edgelet_http::client::{Client as HttpClient, ClientImpl, TokenSource};
use edgelet_http::ErrorKind as HttpErrorKind;

use crate::error::{Error, ErrorKind};
use crate::models::*;
use edgelet_http::client::ClientImpl;

pub struct HostingClient<C>
    where
        C: ClientImpl,
{
    client: Arc<RwLock<Client<C, StaticTokenSource>>>,
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
        let httpClient = HttpClient::new(
            client_impl,
            None as Option<StaticTokenSource>,
            api_version,
            hosting_environment_endpoint,
        )
            .context(ErrorKind::DpsInitialization)?;
        Ok(HostingClient {
            client: Arc::new(RwLock::new(httpClient)),
        })
    }
}