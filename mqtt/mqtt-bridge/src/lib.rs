#![deny(rust_2018_idioms, warnings)]
#![deny(clippy::all, clippy::pedantic)]
#![allow(
    clippy::cognitive_complexity,
    clippy::large_enum_variant,
    clippy::similar_names,
    clippy::module_name_repetitions,
    clippy::use_self,
    clippy::match_same_arms,
    clippy::must_use_candidate,
    clippy::missing_errors_doc
)]

mod bridge;
pub mod client;
mod connectivity;
pub mod controller;
mod persist;
mod rpc;
mod settings;
mod token_source;

pub use crate::controller::BridgeController;
