Feature: Get certificate authorities

Scenario: Returns an empty array if no certificate authorities are registered
	Given the user "test@example.com" is authenticated with the password "t3sTpa55w0rd"
	When the endpoint /cas is called
	Then the status code should be 200
		And the response is an empty array