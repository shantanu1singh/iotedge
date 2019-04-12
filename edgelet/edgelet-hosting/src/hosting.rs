// Copyright (c) Microsoft. All rights reserved.

use std::sync::{Arc, Mutex};

use bytes::Bytes;
use failure::Fail;
use futures::prelude::*;

use edgelet_core::crypto::{Activate, KeyIdentity, KeyStore as CoreKeyStore, Sign, SignatureAlgorithm, Signature};
use edgelet_core::{Digest as ExternalDigest, Error as CoreError, ErrorKind as CoreErrorKind};
use edgelet_http_hosting::{HostingClient, HostingInterface};

pub use crate::error::{Error, ErrorKind};

const ROOT_KEY_NAME: &str = "primary";

/// Represents a key which can sign data.
#[derive(Clone)]
pub struct ExternalKey
//where
//    C: 'static + ClientImpl + Clone,
{
    #[allow(dead_code)]
    hosting: Arc<Mutex<HostingClient>>,
    #[allow(dead_code)]
    identity: KeyIdentity,
    #[allow(dead_code)]
    key_name: String,
}

impl ExternalKeyStore
{
    //    pub fn new() -> Result<Self, Error> {
    //        let hsm = HostingClient::new()?;
    //        ExternalKeyStore::from_hosting_environment(hsm)
    //    }

    pub fn from_hosting_environment(hosting: HostingClient) -> Result<Self, Error> {
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
    pub fn get_active_key(&self) -> Result<ExternalKey, Error> {
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
pub struct ExternalKeyStore
{
    hosting: Arc<Mutex<HostingClient>>,
}

impl CoreKeyStore for ExternalKeyStore
{
    type Key = ExternalKey;

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

impl Activate for ExternalKeyStore
//where
//    C: 'static + ClientImpl + Clone,
{
    type Key = ExternalKey;

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

impl Sign for ExternalKey
//where
//    C: 'static + ClientImpl + Clone,
{
    type Signature = ExternalDigest;

    /// Sign data with this key.
    /// If an identity was given, we will derive a new key from the identity and sign the data.
    /// If an identity was not given, we will sign the data with the stored key.
    fn sign(
        &self,
        signature_algorithm: SignatureAlgorithm,
        data: &[u8],
    ) -> Result<Self::Signature, CoreError> {
//        Ok(ExternalDigest::new(Bytes::from("abc")))

        let identity = match self.identity {
            KeyIdentity::Device =>
                None,
            KeyIdentity::Module(ref m) =>{
                Some(Bytes::from(format!("{}{}", m, self.key_name)))
            }
        };

//        let mut identity = None;
//        match self.identity {
//            KeyIdentity::Device => {},
//            KeyIdentity::Module(ref m) => {
//                let key = format!("{}{}", m, self.key_name);
//                identity = Some(key.as_bytes().clone());
//            }
//        };

        self.hosting
            .lock()
            .expect("Lock failed")
            .sign(
                identity.map( as_bytes()),
                &signature_algorithm.to_string(),
                data
            )
            .map_err(|err| Error::from(err.context(ErrorKind::HostingClient)))
            .map_err(|err| CoreError::from(err.context(CoreErrorKind::KeyStore)))
            .wait()
            .and_then(|digest| {
                match digest {
                    None => Err(CoreError::from(Error::from(ErrorKind::EmptyDigest).context(CoreErrorKind::KeyStore))),
                    Some(t) => {
                        println!(
                            "Signed data: \"{}\"",
                            base64::encode(&t.clone())
                        );

                        Ok(Self::Signature::new(t))
                    }
                }
            })
//            .map(|digest| {
//                digest.is_none()
//                match digest {
//                    None => Error::from(ErrorKind::EmptyDigest),
//                    Some(t) => {
//                        println!(
//                            "Signed data: \"{}\"",
//                            base64::encode(&t.clone())
//                        );
//
//                        Self::Signature {
//                            bytes: t
//                        }
//                    }
//                }
//            })
    }
}
