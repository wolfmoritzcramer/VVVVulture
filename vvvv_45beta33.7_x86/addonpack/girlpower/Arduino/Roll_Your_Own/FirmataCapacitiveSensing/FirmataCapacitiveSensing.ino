/*
  How to use Firmata as a flexible transport prototcol
  in custom firmware (simple).
  
  Make sure you have the CapSense library installed:
  http://playground.arduino.cc//Main/CapacitiveSensor
  
  From the firmata & vvvv workshop http://node13.vvvv.org/
 */

#include <Boards.h>
#include <Firmata.h>

#include <CapacitiveSensor.h>

CapacitiveSensor sensor  = CapacitiveSensor(2, 3);

void setup(){
  sensor.set_CS_AutocaL_Millis(0xFFFFFFFF);
  Firmata.begin();
}

void loop(){

  long sensorValue = sensor.capacitiveSensor(30);

  Firmata.sendAnalog(0,sensorValue);

  delay(10);
}


