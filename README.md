# Apprenda-MongoDB
This is the repository that includes all the details to integrate Apprenda with the MongoDB data store. Using this AddOn, Developers will be able to provision and deprovision MongoDB databases for use in their Apprenda applications.

## Code Repository
- Apprenda.AddOns.MongoDB, the complete source code for the implementation of the AddOn
- Apprenda-MongoDB.Test, a test framework to validate this integration
- Apprenda.AddOns.MongoDB.zip, the zipped file that can be uploaded to the Apprenda Operator Portal (SOC) to enable the MongoDB AddOn

## Integration Steps, Setting up the Apprenda Add-On in the Apprenda Operator Portal

### Configuring Your MongoDB Instance ###
- This add-on assumes you have a MongoDB instance running. To get your MongoDB instance up and running please refer to [these tutorials](https://docs.mongodb.com/manual/installation/#tutorials) and choose the appropriate one for your OS and version of MongoDB.
#### Authenication ####
- By default your MongoDB instance will have authentication turned off. You can use this addon with or without authentication. If you would like to use authentication keep reading below, otherwise continue to the next section.
- To turn on authentication, first create an admin user with full privileges. You can do this from MongoDB shell with the following command: 
```
use admin
db.createUser(
{
    user: "admin",
    pwd: "password",
    roles: [ "root" ]
})
```
- Next, to turn authentication on, add (or modify) the following line in your MongoDB configuration file
```yaml
auth = true
```
- If you don't have already have configuration file, create a file with the line above, save it as mongod.conf, and restart your MongoDB instance with this file in the --config option.
```
	mongod --config /etc/mongdb.conf
```
- Remember this username and password for use in the Add-On later.
- Save your config file and restart your MongoDB instance
#### Set Bind IP ####
- The Add-On will need to be able to access your MongoDB instance through Apprenda in order to function. Open your config file and set the bind IP to the necessary IP addresses.
- If you would like to be able to access your instance from any IP address, set your bind ip as follows by adding (or modifying) the following line if your MongoDB configuration file:
```yaml
bind_ip = 0.0.0.0
```
- Alternatively, you can set your bind IP to a specific IP or list of IPs of your choosing:
```yaml 
bind_ip = [216.58.216.164, 69.89.31.226]
```
- Save your config file and restart your MongoDB instance.

### Configure the Add-On ###
- Use the provided Apprenda.AddOns.MongoDB.zip to upload the Add-On to the Apprenda SOC (aka Operator Portal). You can alternatively build or enhance the provided Visual Studio solution file to create an Add-On that meets your needs.
- Once the Add-On has uploaded, click on edit. 
- The location field in the "General" tab should be the host location of the MongoDB instance. For example mymongodb.cloudapps.net.
- The username and password fields in the "General" tab require the username and password of the user you created above, *if* authentication is enable. If you are not using authentication, leave these fields blank.
- The default port 27017 is attached to the location when connecting to the MongoDB database. If you want to override the port, visit the "Configuration" tab and update the Port field
- Save your changes, and click on Test.
- For the test enter a username and password -- these are NOT your authenticated user credentials, this is a new user which will be created (and then deleted) as a part of the test.
- If the test is successful you are good to go, move on to provisioning your Add-On.

### Using the Add-On ###
- Navigate to your developer portal. On the left, click on Add-Ons
- You should see your Add-On here. Click on it, and click the '+' symbol to provision a new database and user
- In Instance Alias enter an alias for your new database
- Enter a username and password for the new user that will be created as an administrator on the database
- Click on apply
- Once provisioning has complete you can verify the existence of the new database and corresponding user through your MongoDB shell
- You can connect to this new database using the Connection Data provided
- When you deprovision this instance, the new database, anything in it, and the new user will be deleted from your MongoDB instance

**Congratulations, you have just integrated the Apprenda Cloud Platform with the MongoDB AddOn**
- You can learn more about Add-Ons at http://docs.apprenda.com/7-0/addons