Feature: Parameters are validated when adding certificate authorities

Background: 
	Given a valid user is authenticated 

Scenario: Adding a root certificate authority requires a name
	When the endpoint /cas is called with a POST request with the data
		| Property | Value          |
		| Password | 5tr0ngp4$$w0rD |
	Then the status code should be 400

Rule: Adding a root certificate authority requires a password

	Example: Password may not be null
		When the endpoint /cas is called with a POST request with the data
			| Property | Value                 |
			| Name     | Root without password |
		Then the status code should be 400

	Example: Password may not be empty
		When the endpoint /cas is called with a POST request with the data
			| Property | Value                 |
			| Name     | Root without password |
			| Password |                       |
		Then the status code should be 400

Rule: Adding a root certificate requires a sufficient password
	
	Example: An uppercase character is required
		When the endpoint /cas is called with a POST request with the data
			| Property | Value          |
			| Name     | Root CA        |
			| Password | 5tr0ngp4$$word |
		Then the status code should be 400

	Example: A lowercase character is required
		When the endpoint /cas is called with a POST request with the data
			| Property | Value          |
			| Name     | Another root   |
			| Password | 5TR0NGP4$$WORD |
		Then the status code should be 400

	Example: A number is required
		When the endpoint /cas is called with a POST request with the data
			| Property | Value          |
			| Name     | Still going    |
			| Password | STRONGPA$$WORD |
		Then the status code should be 400
	
	Example: A special character is required
		When the endpoint /cas is called with a POST request with the data
			| Property | Value          |
			| Name     | Another 1      |
			| Password | StR0NgP4SsW0Rd |
		Then the status code should be 400

	Example: A length of 12 characters is required
		When the endpoint /cas is called with a POST request with the data
			| Property | Value    |
			| Name     | Rootbeer |
			| Password | P4$$w0Rd |
		Then the status code should be 400

Rule: Validation errors are returned by the API

	Example: Returns a message if the password is null
		When the endpoint /cas is called with a POST request with the data
			| Property | Value                   |
			| Name     | Root without a password |
		Then there is a validation error "'Password' must not be empty." for "Password"

	Example: Returns a message if the password is not strong enough
		When the endpoint /cas is called with a POST request with the data
			| Property | Value                |
			| Name     | Root with a password |
			| Password | p4$$w0rD             |
		Then there is a validation error "Insufficient password strength." for "Password"

	Example: Returns a message if the name is null 
		When the endpoint /cas is called with a POST request with the data
			| Property | Value          |
			| Password | 5tr0ngp4$$W0rD |
		Then there is a validation error "'Name' must not be empty." for "Name"

	Example: Returns a message if the name is empty
		When the endpoint /cas is called with a POST request with the data
			| Property | Value          |
			| Name     |                |
			| Password | &tr0nGp45$worD |
		Then there is a validation error "'Name' must not be empty." for "Name"