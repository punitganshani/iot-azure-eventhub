
console.log('started');

var https = require('https');
var crypto = require('crypto');
var moment = require('moment');
var uuid= require('node-uuid');
var os = require("os");

var deviceName = "alpha"; //os.hostname();
var deviceKeyValue = 'b7lPDtsAoCJC7JggSnu0I82Kj7Gu0QvMX8XkNKcOpx0=';          
     
for (var counter = 0; counter < 5; counter++){
      var payload =  {  MeasuredValue: randomInt(25, 29), 
                        DeviceName: deviceName, 
                        RecordedAt: new Date(), 
                        SensorType: "Temperature", 
                        MessageId: uuid.v4() 
                     };
      SendTemperature(payload);
}

function SendTemperature(payload) {
      
      var payLoadString = JSON.stringify(payload);
      console.log(payLoadString);
      // Event Hubs parameters
      var namespace = 'iot-bus';
      var hubname ='inbox';
      
      // Shared access key (from Event Hub configuration) 
      var deviceKey = deviceName; 
       
      var my_uri = 'https://' + namespace + '.servicebus.windows.net' + '/' + hubname  + '/messages';
      
      function create_sas_token(uri, key_name, key)
      {
          // Token expires in one hour
          var expiry = moment().add(1, 'hours').unix();
      
          var string_to_sign = encodeURIComponent(uri) + '\n' + expiry;
          var hmac = crypto.createHmac('sha256', key);
          hmac.update(string_to_sign);
          var signature = hmac.digest('base64');
          var token = 'SharedAccessSignature sr=' + encodeURIComponent(uri) + '&sig=' + encodeURIComponent(signature) + '&se=' + expiry + '&skn=' + key_name;
      
          return token;
      }

      var my_sas = create_sas_token(my_uri, deviceKey, deviceKeyValue)

      // console.log(my_sas);
      
      // Send the request to the Event Hub
      
      var options = {
        hostname: namespace + '.servicebus.windows.net',
        port: 443,
        path: '/' + hubname + '/messages',
        method: 'POST',
        headers: {
          'Authorization': my_sas,
          'Content-Length': payLoadString.length,
          'Content-Type': 'application/atom+xml;type=entry;charset=utf-8'
        }
      };

      var req = https.request(options, function(res) {
        
      });

      req.on('error', function(e) {
        console.error(e);
      });
      
      req.write(payLoadString);
      req.end();
}
 
 
function randomInt (low, high) {
    return Math.floor(Math.random() * (high - low) + low);
}