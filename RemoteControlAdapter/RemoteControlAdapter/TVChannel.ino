#define channel1 2
#define channel4 3
#define channel5 4
#define channel6 5
#define channel8 6
#define channel10 7
#define power 8
#define volumeUp 9
#define volumeDown 10
#define nextChannel 11

/*
Power 0
VolumeUp 1
VolumeDown 2
NextChanel 3  
Chanel1 4
Chanel4 5
Chanel5 6
Chanel6 7
Chanel8 8
Chanel10 9

*/

int data = 0;  // receive data

void setup() {
  Serial.begin(9600);
  pinMode(channel1, OUTPUT);
  pinMode(channel4, OUTPUT);
  pinMode(channel5, OUTPUT);
  pinMode(channel6, OUTPUT);
  pinMode(channel8, OUTPUT);
  pinMode(channel10, OUTPUT);
  pinMode(power, OUTPUT);
  pinMode(volumeUp, OUTPUT);
  pinMode(volumeDown, OUTPUT);
  pinMode(nextChannel, OUTPUT);
  
}

void loop() {
  if (Serial.available() > 0) {
    data = Serial.read();
    data = data - 48;
    Serial.print(data, DEC);
    switch (data) {
      case 0:
        digitalWrite(power, HIGH);
        delay(500);
        digitalWrite(power, LOW);
        break;
      case 1:
        digitalWrite(volumeUp, HIGH);
        delay(200);
        digitalWrite(volumeUp, LOW);
        delay(200);
        digitalWrite(volumeUp, HIGH);
        delay(200);
        digitalWrite(volumeUp, LOW);
        delay(200);
        digitalWrite(volumeUp, HIGH);
        delay(200);
        digitalWrite(volumeUp, LOW);
        delay(200);
        digitalWrite(volumeUp, HIGH);
        delay(200);
        digitalWrite(volumeUp, LOW);
        break;
      case 2:
        digitalWrite(volumeDown, HIGH);
        delay(200);
        digitalWrite(volumeDown, LOW);
        delay(200);
        digitalWrite(volumeDown, HIGH);
        delay(200);
        digitalWrite(volumeDown, LOW);
        delay(200);
        digitalWrite(volumeDown, HIGH);
        delay(200);
        digitalWrite(volumeDown, LOW);
        delay(200);
        digitalWrite(volumeDown, HIGH);
        delay(200);
        digitalWrite(volumeDown, LOW);
        break;
      case 3:
        digitalWrite(nextChannel, HIGH);
        delay(500);
        digitalWrite(nextChannel, LOW);
        break;
      case 4:
        digitalWrite(channel1, HIGH);
        delay(500);
        digitalWrite(channel1, LOW);
        break;
      case 5:
        digitalWrite(channel4, HIGH);
        delay(500);
        digitalWrite(channel4, LOW);
        break;
      case 6:
        digitalWrite(channel5, HIGH);
        delay(500);
        digitalWrite(channel5, LOW);
        break;
      case 7:
        digitalWrite(channel6, HIGH);
        delay(500);
        digitalWrite(channel6, LOW);
        break;
      case 8:
        digitalWrite(channel8, HIGH);
        delay(500);
        digitalWrite(channel8, LOW);
        break;
      case 9:
        digitalWrite(channel10, HIGH);
        delay(500);
        digitalWrite(channel10, LOW);
        break;
      case 10:
        digitalWrite(channel10, HIGH);
        delay(5000);
        digitalWrite(channel10, LOW);
        break;
    }
  }
}
