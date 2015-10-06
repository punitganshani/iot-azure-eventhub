/* NOTE:
    Developed to work on Intel Galileo Gen 2 

    Temperature sensor:  A0 pin
    LED: D6
 */
console.log("Started at:" + new Date().toString());

/* Dependencies */
var azure = require('azure');
var Galileo = require("galileo-io");
var five = require("johnny-five");
var uuid= require('node-uuid');
var https = require('https');
var crypto = require('crypto');
var moment = require('moment');
var os = require("os");

/* Sensor Constants */
var pinTemperature = "A0";
var refreshRate = 60000;

var deviceName = "galileo"; //os.hostname();

/* Azure setup - Event Hub */
var namespace = 'iot-bus';
var hubname ='inbox';
var my_key_name =deviceName; 
var my_key = 'iZOB7LIBsVRabqKm0TWmIqis3FLbkj0woMaCAXI6m98=';
var my_uri = 'https://' + namespace + '.servicebus.windows.net' + '/' + hubname  + '/messages';

/* Azure setup - Topic */
var topicName = "t.to.devices";
var shared_listener_key_name='DeviceSharedKey';
var shared_listener_key_value='DeviceSharedKey';
var serviceBusConnectionString = 'Endpoint=sb://' +namespace + '.servicebus.windows.net/;SharedAccessKeyName='+ shared_listener_key_name+ ';SharedAccessKey=' + shared_listener_key_value;
var subscriptionName = "device." + deviceName;
var subscriptionCriteria = "deviceName = '" + deviceName + "'";
var filterName = 'FilterFor' + deviceName;
var serviceBusService = azure.createServiceBusService(serviceBusConnectionString);
var rule = {
    deleteDefault: function () {
        serviceBusService.deleteRule(topicName,
            subscriptionName,
            azure.Constants.ServiceBusConstants.DEFAULT_RULE_NAME,
            rule.handleError);
    },
    create: function () {
        var ruleOptions = {
            sqlExpressionFilter: subscriptionCriteria
        };
        rule.deleteDefault();
        serviceBusService.createRule(topicName,
            subscriptionName,
            filterName,
            ruleOptions,
            rule.handleError);
    },
    handleError: function (error) {
        if (error) {
            console.log(error);
        }
    }
}//rule

console.log('Connected to Azure');

var board = new five.Board({
    io: new Galileo()
});

board.on("ready", function () {

    console.log("Connection Established with IoT device");

    var temperature = new five.Temperature({
        controller: "GROVE",
        pin: pinTemperature,
        freq: refreshRate
    });

    var led = new five.Led(6);
    led.off();

    var lcd = new five.LCD({
        controller: "JHD1313M1"
    });

    temperature.on("data", function () {

        var f = temperature.fahrenheit;
        var r = linear(0x00, 0xFF, f, 100);
        var g = linear(0x00, 0x00, f, 100);
        var b = linear(0xFF, 0x00, f, 100);

        console.log("Temp measured (F): ", f);
        var fahrenheit = f.toFixed(2);
        lcd.bgColor(r, g, b).cursor(0, 0).print(fahrenheit + " F");

        var payload =  { MeasuredValue: fahrenheit, DeviceName: deviceName, RecordedAt: new Date(), SensorType: "Temperature", MessageId: uuid.v4() }
               
        SendMeasurementToAzure(payload);
        
    }); //temperature.on("data")


    serviceBusService.deleteSubscription(topicName, subscriptionName, function (error) {
        if (error) {
            console.log("Subscription Deletion Error :" + error);
            createMessageSubscription();
        }
        else {
            console.log('Subscription deleted : ' + subscriptionName);
            createMessageSubscription();
        }
    }); //deleteSubscription


    function createMessageSubscription() {

        serviceBusService.createSubscription(topicName, subscriptionName, function (error) {
            console.log("Topic subscription created with name: " + subscriptionName);
            if (!error) {
                rule.create();
                setInterval(receiveMessagesFromAzure, refreshRate);
            }
            else {
                console.log("Subscription Creation Error :" + error);
            }
        });
    } //createMessageSubscription

    function SendMeasurementToAzure(payload){
        
        var my_sas = create_sas_token(my_uri, my_key_name, my_key)
        var options = {
                hostname: namespace + '.servicebus.windows.net',
                port: 443,
                path: '/' + hubname + '/messages',
                method: 'POST',
                headers: {
                    'Authorization': my_sas,
                    'Content-Length': payload.length,
                    'Content-Type': 'application/atom+xml;type=entry;charset=utf-8'
                }
            };
            
        var req = https.request(options, function(res) {
        console.log("statusCode: ", res.statusCode);
            //console.log("headers: ", res.headers);
        
        res.on('data', function(d) {
            //process.stdout.write(d);
            });
        });
    
        req.on('error', function(e) {
            console.error(e);
        });
        
        req.write(payload);
        req.end();
        console.log("done");

    }

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

    function receiveMessagesFromAzure() {

        serviceBusService.receiveSubscriptionMessage(topicName, subscriptionName, function (error, receivedMessage) {
            if (!error) {
                // Message received and deleted
                console.log(receivedMessage);
                console.log("AlertType: " + receivedMessage.customProperties.alerttype);
                if (receivedMessage.customProperties.alerttype == "HIGH"
                   && receivedMessage.customProperties.sensortype == "Temperature") {
                    console.log('Temperature HIGH detected. AC to be turned ON');
                    led.brightness(255);
                    led.on();
                    lcd.cursor(1, 0).print("ACT: AC Turn ON ");

                }
                else if (receivedMessage.customProperties.alerttype == "LOW"
                    && receivedMessage.customProperties.sensortype == "Temperature") {
                    console.log('Temperature LOW detected. AC to be turned OFF');
                    led.brightness(128);
                    led.fade(128, 2000);
                    led.stop();
                    led.off();
                    lcd.cursor(1, 0).print("ACT: AC Turn OFF");
                }
                else {
                    console.log('unknown message');
                }
            }
        });
    } //receiveMessagesFromAzure
}); //board.on('ready')

// [Linear Interpolation](https://en.wikipedia.org/wiki/Linear_interpolation)
function linear(start, end, step, steps) {
    return (end - start) * step / steps + start;
}