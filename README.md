# Apprenda-MongoDB
This is the repository that includes all the details to integrate Apprenda with the MongoDB data store. Using this AddOn, Developers will be able to provision and deprovision MongoDB databases for use in their Apprenda applications.

## Code Repository
- Apprenda.AddOns.MongoDB, the complete source code for the implementation of the AddOn
- Apprenda-MongoDB.Test, a test framework to validate this integration
- Apprenda.AddOns.MongoDB.zip, the zipped file that can be uploaded to the Apprenda Operator Portal (SOC) to enable the MongoDB AddOn

## Integration Steps, Setting up the Apprenda Add-On in the Apprenda Operator Portal
- Use the provided Apprenda.AddOns.MongoDB.zip to upload the Add-On to the Apprenda SOC (aka Operator Portal). You can alternatively build or enhance the provided Visual Studio solution file to create an Add-On that meets your needs.
- Once the Add-On is uploaded in Apprenda, edit it 
- This add-on assumes that you already have a MongoDB instace running, configured with authentication, and one admin user. The admin username and password are needed to create client databases. 
- The location field in the "General" tab should be the host location of the MongoDB instance. For example mymongodb.cloudapps.net. The default port 27017 is attached to the location when connecting to the MongoDB database. If you want to override the port, visit the "Configuration" tab and update the Port field
- Save the Add-On
- You can learn more about Add-Ons at http://docs.apprenda.com/7-0/addons

## Integration Steps, Setting up the Apprenda Add-On in the Apprenda Developer Portal
- Documentation coming soon!

**Congratulations, you have just integrated the Apprenda Cloud Platform with the MongoDB AddOn**
