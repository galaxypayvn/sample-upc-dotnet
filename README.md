1. Required 
- Node JS
https://nodejs.org/en/download/

- .Net 6.0
https://dotnet.microsoft.com/en-us/download/dotnet/6.0


2. Edit setting in file appsetting.json

- UPC.Salt to sign and verify signature.
- UPC.APIKey to the key that you receive from UPC.


3. Run
- Open solution with Visual Studio
- Start with debug or without debug.


========================================================================

Live Demo
https://uat-merchant.galaxypay.vn

API Documents
https://github.com/galaxypayvn/sample-upc-dotnet/tree/master/Documents


========================================================================

CHANGE LOGS

Version 3.3     14-09-2022
- Add payment providers (GPAY, 2C2P Hub)

Version 3.2     12-09-2022
- Update API fields
- Add API Pay with Options

Version 3.1     07-06-2022
- Add support for Hosted Merchant

Version 3.0	    06-05-2022
BREAKING CHANGE
-	Update Signature logic
-	Update API Request & Response
-	Add Extra data