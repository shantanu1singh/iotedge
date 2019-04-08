// Copyright (c) Microsoft. All rights reserved.

#![deny(rust_2018_idioms, warnings)]
#![deny(clippy::all, clippy::pedantic)]
#![allow(clippy::module_name_repetitions, clippy::use_self)]

//mod certificate_properties;
//mod crypto;
mod error;
pub mod hosting;

//pub use crypto::{Certificate, Crypto};
pub use error::{Error, ErrorKind};
pub use hosting::{ExternalKey, ExternalKeyStore};
