/*
  ArduLog V1.0
  Requires Arduino V1.0 and above

  Written by Hobbytronics Ltd (www.hobbytronics.co.uk)
  This example code is in the public domain.
  
  Simple Data Logging program for the Hobbytronics ArduLog board. Please feel free to amend and adapt.
  
  Appends all received serial data to the log file
  Uses pin A1 to start/stop logging
  
  Configuration is determined by config.txt file which should have the following entries
  
  BAUD xxx
  FILE filename_prefix
  
  Green LED lights up to indicate logging has started, extinguishes when logging stopped
  Orange LED indicates data activity

*/
 
#include <SD.h>

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
    
  if(!digitalRead(CardDetect))
  {
    // CardDetect LOW - a card is inserted 
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
    digitalWrite(statusLED, HIGH);  // Logging started 
  }
  else
  {
    Serial.println("No card detected");     
    
  }
  currentTime = millis();
  cloopTime = currentTime;
}

void loop()
{
  
   if (Serial.available() > 0) 
   {
     // Read byte and save to file
     char inByte = Serial.read(); 
     myFile.print(inByte); 
   }
   
   currentTime = millis();
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

