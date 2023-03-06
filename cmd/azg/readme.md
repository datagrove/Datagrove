go get -u -d github.com/Azure/azure-sdk-for-go/...

az ad sp create-for-rbac --role Contributor \
    --scopes /subscriptions/ed56b77a-1d93-45ec-862a-6314665e907c \
    --sdk-auth > quickstart.auth

 WARNING: Option '--sdk-auth' has been deprecated and will be removed in a future release.
WARNING: Creating 'Contributor' role assignment under scope '/subscriptions/ed56b77a-1d93-45ec-862a-6314665e907c'
WARNING: The output includes credentials that you must protect. Be sure that you do not include these credentials in your code or check the credentials into your source control. For more information, see https://aka.ms/azadsp-cli   


go get -u -d github.com/Azure-Samples/azure-sdk-for-go-samples/quickstarts/deploy-vm/...
