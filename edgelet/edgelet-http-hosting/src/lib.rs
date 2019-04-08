// Copyright (c) Microsoft. All rights reserved.

#![deny(rust_2018_idioms, warnings)]
#![deny(clippy::all, clippy::pedantic)]
#![allow(clippy::module_name_repetitions, clippy::use_self)]

use hyper::{Body, Response};

pub mod client;
pub mod error;
pub mod models;

pub use client::{HostingClient, HostingInterface};
pub use error::{Error, ErrorKind};
pub use models::*;

pub trait IntoResponse {
    fn into_response(self) -> Response<Body>;
}

impl IntoResponse for Response<Body> {
    fn into_response(self) -> Response<Body> {
        self
    }
}
