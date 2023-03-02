Node wrapper for pkg/dbcheckout and package/dbdeli_view

```


npx dbdeli [dir]
```

# Building
At a high level
* Build dbdelijs (gui)
* Build dbdeli.go (server, embeds dbdelijs)
* Build dbdeli npm package (installer)

# todo
* This should either have etcd type features or work with etcd
* The api should change to support a list of resources to lock
* Potentially a grpc and/or http api; not clear how to long polling though (to wait on locks)


1. get unlicensed v10.bak
2. dbdeli load dbdeli v10.bak  # create the golden copy, license it.
3. dbdeli build     # this will first create the golden backup, then copy it to N copies.


dotnet test --filter "Name~I_navigate_to_all_staff_menu_items_in_RiSE__" --list-tests