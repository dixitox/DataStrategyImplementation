# Steps to Deploy the DS Toolkit 
### Sandbox for the vat-tax model
1. Deploy the Data Strategy IP Kit with the [feature flag](https://github.com/rachaelph/Data-Strategy-Reference-Implementation/blob/main/DeliveryIP_GitHub/variables/general_feature_flags/feature_flags_prod.json) for the production environment for the DeployDataScienceToolkit to be true.
2. Create a repository in their desired github organization and copy over the the [datastrategy-acts](https://github.com/microsoft/datastrategy-acts) repository and name it vat-tax.
3. [Create an access token](https://www.theserverside.com/blog/Coffee-Talk-Java-News-Stories-and-Opinions/How-to-create-a-GitHub-Personal-Access-Token-example#:~:text=To%20create%20a%20personal%20access%20token%20in%20GitHub%2C,access%20token%20in%20the%20%E2%80%9CNote%E2%80%9D%20field.%20More%20items) for the organization using someone in the organization.
4. Add in these secrets to your Data-Strategy-Reference-Implementation repository:
    - USER_EMAIL: the user email that you used for step 3.
    - USER_NAME: user name that is associated with the user email from step 3.
	- ACCESS_TOKEN: this is a personal access token you created in step 3.
5. Must turn on the following feature flags to true, which is found here: Data-Strategy-Reference-Implementation/DeliveryIP_GitHub/variables/general_feature_flags/feature_flags_dev.json
Latest
	- DeployDevEnvironment
	- DeployResourcesWithPublicAccess or DeployWithCustomNetworking
	- ServicePrincipalHasOwnerRBACAtSubscription
	- DeployDataLake
	- DeployLandingStorage
	- DeployADF
	- DeployADFArtifacts
	- DeploySynapse
	- DeploySynapsePools
	- DeploySynapseArtifacts
	- DeployMLWorkspace
	- DeployMLCompute
	- DeployKeyVault
 6. Must change the following variable names to what you would like in your sandbox, which is found here: Data-Strategy-Reference-Implementation/DeliveryIP_GitHub/variables/general_variables/variables_dev.json
	- azureResourceLocation
	- PrimaryRgName
	- AADGroup_Name
	- synapseWorkspaceName
	- dataFactoryName
	- dataLakeName
	- landingStorageName
	- MlRgName
	- mlWorkspaceName
	- mlWorkspaceKeyVaultName
	- mlStorageName
	- mlAppInsightsName
	- mlContainerRegistryName
	- keyVaultName
7. If you have to have a VM and/or a custom network, you need to add the following secrets: 
	- VMUSERNAME
	- VMPASSWORD
	- DNSZONESUBSCRIPTIONID
8. Add this to the bottom of the job called deploy-dev-env-variables:
	- VM_USERNAME: ${{ secrets.VMUSERNAME }}
    - VM_PASSWORD: ${{ secrets.VMPASSWORD }}
    - DNS_ZONE_SUBSCRIPTION_ID: ${{ secrets.DNSZONESUBSCRIPTIONID }}
9. Go to your UI link and hit Deploy to Sandbox
### Production for the vat-tax model
1. Create the production repository in the organization of choice: vat-tax-production
2. Go to your UI link and hit Deploy to Production