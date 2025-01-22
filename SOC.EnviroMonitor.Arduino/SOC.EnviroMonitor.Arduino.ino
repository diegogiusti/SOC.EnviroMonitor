#include <ArduinoJson.h>
#include "DHT.h"

#define PIR_PIN 7
#define MIC_PIN A0
#define BRI_PIN A1
#define DHTPIN 2

#define DHTTYPE DHT11


DHT dht(DHTPIN, DHTTYPE);

void setup() {
  Serial.begin(9600);
  dht.begin();
  pinMode(PIR_PIN, INPUT);
}

void loop() {
  delay(500);
  float h = dht.readHumidity();
  float t = dht.readTemperature();
  
  if (isnan(h) || isnan(t)) {
    Serial.println(F("Failed to read from DHT sensor!"));
    return;
  }

  float hic = dht.computeHeatIndex(t, h, false);

  JsonDocument doc;

  doc["temperature"] = t;
  doc["humidity"] = h;
  doc["pir"] = digitalRead(PIR_PIN);
  doc["brightness"] = analogRead(BRI_PIN);
  doc["mic"] = analogRead(MIC_PIN);

  serializeJson(doc, Serial);
  Serial.println();
}
