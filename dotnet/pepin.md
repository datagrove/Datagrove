
# Using pepin with an existing project

You can install pepin globally to work in any project or in the project (convenient for CI pipelines). `pepin build` will use the defaults for the current directory. You may use a configuration file to change the default behavior.

# Using pepin.config.json

By default compiles all the tests into a single output file and uses steps in every namespace. In a big testing project this can lead to conflicts where steps in multiple namespaces both match a single step. You can use pepin.config.json to split up your features into different files and namespaces, and then each output file can be configured to pull steps from specific namespaces.

```json
{
    "space": {
        "myspace": {
            "features": [
                "path to directory with feature files for this space"
            ],
            "steps": [
                "path to a directory with step code for this space"
            ]
        },
    }
}
```

# Why?

Unlike Specflow which acts like an interpreter for your gherkin code, pepin build will convert your feature files to generated c# code that fits whatever automated process you want. It will be faster (how much faster depends a lot on the test).