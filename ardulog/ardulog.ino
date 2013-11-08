/*

  Configuration is determined by config.txt file which should have the following entries
  
  BAUD xxx
  FILE filename_prefix
  
  Green LED lights up to indicate logging has started, extinguishes when logging stopped
  Orange LED indicates data activity

*/
 
#include <SD.h>
#include <Wire.h>

const int CardDetect = A2;     // the card detect pin
const int statusLED = 5;       // Green Status LED on pin 5
const int file_flush = 2;      // Force data write to file after x seconds 

File myFile;

// Struct to hold config info from config.txt file
struct {
  unsigned int baud;    // baud rate
  char fileprefix[20];  // filename prefix  
  char filename[20];    // filename to log data
} config;

unsigned long currentTime;
unsigned long cloopTime;
unsigned long cloopBmpTime;

const float p0 = 101325;     // Pressure at sea level (Pa)

void setup()
{
  int i;
  pinMode(CardDetect, INPUT);  // Card Detect
  pinMode(statusLED, OUTPUT);  // Status LED
  
  // Default config settings - override with config file "config.txt" if required
  config.baud = 0;
  strcpy(config.fileprefix, "LOG.");  
  strcpy(config.filename, "LOG.001");
  
  // Note that even if it's not used as the CS pin, the hardware SS pin 
  // (10 on most Arduino boards, 53 on the Mega) must be left as an output 
  // or the SD library functions will not work. 
  pinMode(10, OUTPUT); 
  
  // Wait for card to be present
  // CardDetect LOW - a card is inserted
  while(digitalRead(CardDetect))
  {
      Serial.println("No card detected");
      delay(1000);
  }

  if (!SD.begin(10)) {
    return;
  }    
  read_config_file();

  if(config.baud > 0)
  {
      Serial.begin(config.baud);
  }
  else
  {
      // Use default baud
      Serial.begin(9600);      
  }
  
  Serial.println("ArduLog..OK");     
  openlogfile();    
  
  Serial.println("Starting bmp");
  // initialise bmp pressure sensor
  Wire.begin();
  bmp085Calibration();

  Serial.println("Starting logging");    
  digitalWrite(statusLED, HIGH);  // Logging started 
  
  currentTime = millis() % 1000;
  cloopTime = currentTime;
  cloopBmpTime = currentTime;
}

String gpsData;

void loop()
{
   while (Serial.available() > 0) 
   {
     // Read byte and save to file
     char inByte = Serial.read();
     
     if (inByte == '$')
     {
       myFile.print("GPS,");
       myFile.print(gpsData);
       gpsData = "";
     }
     
     gpsData += inByte;
   }

   currentTime = millis();
   
   if (currentTime >= (cloopBmpTime + 1000)) // log temp & pressure data every 1000ms
   {
     short temperature = bmp085GetTemperature(bmp085ReadUT());
     long pressure = bmp085GetPressure(bmp085ReadUP());
     float altitude = (float)44330 * (1 - pow(((float) pressure/p0), 0.190295));

     myFile.print("BMP085,");
     myFile.print(currentTime, DEC); // time since boot
     myFile.print(",");
     myFile.print(temperature, DEC); // *0.1dec C
     myFile.print(",");
     myFile.print(pressure, DEC); // Pa
     myFile.print(",");
     myFile.println(altitude, 2); // m
   }
   
   if(currentTime >= (cloopTime + (file_flush*1000)))
   {
       // Flush data to file after (file_flush) seconds
       myFile.flush();
       cloopTime = currentTime;  // Updates cloopTime
   }  
}

void openlogfile()
{
    // Search for next log filename
    get_new_logfile();    
    
    myFile = SD.open(config.filename, FILE_WRITE);

    // if the file opened okay, output the name
    if (myFile) 
    {
      Serial.print("Log file: ");
      Serial.print(config.filename);
    }  
    else 
    {
      // if the file didn't open, print an error:
      Serial.print("Error opening log file: ");
      Serial.println(config.filename);      
    }  
}  

