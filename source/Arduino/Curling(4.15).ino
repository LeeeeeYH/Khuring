// C++ code
//
int A = 7; //다음 동작 버튼
int B = 6; //선택 버튼
int i = 1; //단계 구분 변수

void setup()
{
  pinMode(A, INPUT);   //A입력받기
  pinMode(B, INPUT);   //B입력받기
  pinMode(A1, INPUT_PULLUP); // A1 위
  pinMode(A2, INPUT_PULLUP); // A2 아래
  pinMode(A3, INPUT_PULLUP); // A3 오른쪽
  pinMode(A4, INPUT_PULLUP); // A4 왼쪽
  digitalWrite(A, HIGH);
  digitalWrite(B, HIGH);   //A,B 는 버튼이 누르기 전 HIGH값을 갖고 있음
  
  Serial.begin(9600); //시리얼 통신 속도:9600
}

void loop()
{ 
  //1.조이스틱을 통해 방향 조절(x축)
  //1을 받게 되면 각도의 값이 양의 방향(오른쪽으로 1만큼 이동)으로 화살표 움직임
  //-1을 받게 되면 각도의 값이 음의 방향(왼쪽으로 1만큼 이동)으로 화살표 움직임
  if (i ==1)
  {
    if (digitalRead(A1)==LOW){
      Serial.write(1);
    }
    if (digitalRead(A2)==LOW){
      Serial.write(-1);
    }
    Select();  //B버튼으로 값 결정
    Next();  //A버튼으로 넘어감
  }  
  
  
  //2.조이스틱을 통해 회전 조절
  //2을 받게 되면 유니티에서 양의 방향(오른쪽)으로 화살표 움직임
  //-2을 받게 되면 유니티에서 음의 방향(왼쪽)으로 화살표 움직임
  if(i==2)
  {
    if (digitalRead(A3)==LOW){
      Serial.write(2);
      Serial.flush();
    }
    if (digitalRead(A4)==LOW){
      Serial.write(-2);
      Serial.flush();
    }
    Select(); //B버튼으로 값 결정
    Next();  //A버튼으로 넘어감
  }
  
  
  //3.속도 게이지 버튼 조절
  //게이지 바가 움직일 경우 원하는 순간에 버튼을 눌러서 멈춤
  if(i==3)
  {
    Select();
    Next();
  }
  
  //4.스위핑
  // 스위핑을 하면 그 수 만큼 비례하여 유니티에서 마찰력 감소
  // 이후 i를 1로 초기화해주면 다음 턴으로 넘어가는경우 다시 반복
  if(i==4)
  {
    
    Serial.write();
    Serial.flush();
    i=1;
  }
  
}

void Select()
{
  if (digitalRead(B) == LOW)
  {
    Serial.write(10); //B버튼이 눌러지는 경우 10의 데이터 값을 받음
    Serial.flush(); //B버튼이 눌러지면 그 값으로 설정
    delay(100);
  }
}
void Next()
{
  if (digitalRead(A) == LOW)
  {
    Serial.write(5); //A버튼이 눌러지는 경우 5의 데이터 값을 받음
    Serial.flush(); //A버튼이 눌러지면 다음 설정으로 넘어감
    delay(100);
  } 
  i++;
}
