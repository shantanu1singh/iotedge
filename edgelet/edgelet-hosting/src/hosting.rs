// Copyright (c) Microsoft. All rights reserved.

use std::sync::{Arc, Mutex};

use bytes::Bytes;
use failure::Fail;

use edgelet_core::crypto::{
    Activate, KeyIdentity, KeyStore as CoreKeyStore, Sign, SignatureAlgorithm,
};
use edgelet_core::{Digest as ExternalDigest, Error as CoreError, ErrorKind as CoreErrorKind};
use edgelet_http::client::ClientImpl;
use edgelet_http_hosting::HostingClient;

pub use crate::error::{Error, ErrorKind};

const ROOT_KEY_NAME: &str = "primary";

/// Represents a key which can sign data.
#[derive(Clone)]
pub struct ExternalKey<C>
where
    C: 'static + ClientImpl + Clone,
{
    #[allow(dead_code)]
    hosting: Arc<Mutex<HostingClient<C>>>,
    #[allow(dead_code)]
    identity: KeyIdentity,
    #[allow(dead_code)]
    key_name: String,
}

impl<C> ExternalKeyStore<C>
where
    C: 'static + ClientImpl + Clone,
{
    //    pub fn new() -> Result<Self, Error> {
    //        let hsm = HostingClient::new()?;
    //        ExternalKeyStore::from_hosting_environment(hsm)
    //    }

    pub fn from_hosting_environment(hosting: HostingClient<C>) -> Result<Self, Error> {
        Ok(ExternalKeyStore {
            hosting: Arc::new(Mutex::new(hosting)),
        })
    }

    /// Activate and store a private key in the external key store.
    pub fn activate_key(&self, _key_value: &Bytes) -> Result<(), Error> {
        //        self.hosting
        //            .lock()
        //            .expect("Lock on KeyStore TPM failed")
        //            .activate_identity_key(key_value)?;
        Ok(())
    }

    /// Get a ExternalKey which will sign data.
    pub fn get_active_key(&self) -> Result<ExternalKey<C>, Error> {
        Ok(ExternalKey {
            hosting: Arc::clone(&self.hosting),
            identity: KeyIdentity::Device,
            key_name: ROOT_KEY_NAME.to_string(),
        })
    }
}

/// The External Key Store.
/// Activate a private key, and then you can use that key to sign data.
#[derive(Clone)]
pub struct ExternalKeyStore<C>
where
    C: 'static + ClientImpl + Clone,
{
    hosting: Arc<Mutex<HostingClient<C>>>,
}

impl<C> CoreKeyStore for ExternalKeyStore<C>
where
    C: 'static + ClientImpl + Clone,
{
    type Key = ExternalKey<C>;

    /// Get a key which will derive and sign data.
    fn get(&self, identity: &KeyIdentity, key_name: &str) -> Result<Self::Key, CoreError> {
        match *identity {
            KeyIdentity::Device => self
                .get_active_key()
                .map_err(|err| CoreError::from(err.context(CoreErrorKind::KeyStore))),
            KeyIdentity::Module(ref m) => {
                if key_name.is_empty() || m.is_empty() {
                    Err(ErrorKind::EmptyStrings)
                        .map_err(|err| Error::from(err.context(ErrorKind::HostingClient)))
                        .map_err(|err| CoreError::from(err.context(CoreErrorKind::KeyStore)))?;
                }
                Ok(ExternalKey {
                    hosting: Arc::clone(&self.hosting),
                    identity: identity.clone(),
                    key_name: key_name.to_string(),
                })
            }
        }
    }
}

impl<C> Activate for ExternalKeyStore<C>
where
    C: 'static + ClientImpl + Clone,
{
    type Key = ExternalKey<C>;

    fn activate_identity_key<B: AsRef<[u8]>>(
        &mut self,
        identity: KeyIdentity,
        _key_name: String,
        key: B,
    ) -> Result<(), CoreError> {
        if identity != KeyIdentity::Device {
            Err(ErrorKind::NoModuleActivation)
                .map_err(|err| Error::from(err.context(ErrorKind::HostingClient)))
                .map_err(|err| CoreError::from(err.context(CoreErrorKind::KeyStore)))?;
        }
        self.activate_key(&Bytes::from(key.as_ref()))
            .map_err(|err| CoreError::from(err.context(CoreErrorKind::KeyStore)))
    }
}

impl<C> Sign for ExternalKey<C>
where
    C: 'static + ClientImpl + Clone,
{
    type Signature = ExternalDigest;

    /// Sign data with this key.
    /// If an identity was given, we will derive a new key from the identity and sign the data.
    /// If an identity was not given, we will sign the data with the stored key.
    fn sign(
        &self,
        _signature_algorithm: SignatureAlgorithm,
        _data: &[u8],
    ) -> Result<Self::Signature, CoreError> {
        Ok(ExternalDigest::new(Bytes::from("abc")))

        //        match self.identity {
        //            KeyIdentity::Device => self
        //                .hosting
        //                .lock()
        //                .expect("Lock failed")
        //                .sign_with_identity(data)
        //                .map_err(|err| Error::from(err.context(ErrorKind::HostingClient)))
        //                .map_err(|err| CoreError::from(err.context(CoreErrorKind::KeyStore))),
        //            KeyIdentity::Module(ref _m) => self
        //                .hosting
        //                .lock()
        //                .expect("Lock failed")
        //                .derive_and_sign_with_identity(
        //                    data,
        //                    format!(
        //                        "{}{}",
        //                        match self.identity {
        //                            KeyIdentity::Device => "",
        //                            KeyIdentity::Module(ref m) => m,
        //                        },
        //                        self.key_name
        //                    )
        //                        .as_bytes(),
        //                )
        //                .map_err(|err| Error::from(err.context(ErrorKind::HostingClient)))
        //                .map_err(|err| CoreError::from(err.context(CoreErrorKind::KeyStore))),
        //        }
    }
}
