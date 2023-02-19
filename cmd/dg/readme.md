User journey tests require fresh databases. This webserver manages reservations so that tests can obtain a fresh database, and then restore the database snapshot.

initially only sql server is supported. intention is to support other databases eventually.

