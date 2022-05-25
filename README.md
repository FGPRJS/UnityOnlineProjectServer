# [접속하기](http://mypofol.shop)
- 컨텐츠 다운로드에 시간이 걸리고, 응답시간이 너무 길어서 접속 할 수 없다는 메시지가 뜰 수 있으나 잠시만 기다려 주세요.
- Exception alert 발생시, 캐시를 지워주세요.

# UnityOnlineProject

AWS LightSail Server

**[클라이언트 프로젝트 참조](https://github.com/FGPRJS/UnityOnlineProject)**


## 프로젝트 개요

**프로젝트 배경**
- Unity WebGL 클라이언트와 연동되는 .Net Socket 서버 만들어보기
- TCP/IP 소켓을 직접 사용하여 Client와 메시지를 나눌 수 있는 Server를 만들어보기
- Client와의 동기화로 채널에 존재하는 모든 Client들의 행동을 동기화해보기

**프로젝트를 통해 얻고자 하는 것**
- AWS로 Socket기반 Server 만들어보기
- 기존에 사용하던 방식으로 Client와의 통신 데이터를 읽고, 써보기
- 여러 Client 들의 정보를 받고, 각 Client들에게 필요한 정보들을 발신하기
- 낮은 사양으로도 해당 정보 교환이 너무 느리지 않게 하기 (온라인 플레이를 할 수 있을 정도)
- 사용 가능한 메시지 프로토콜 만들어보기

## 사용 기술
- .Net Core 3.1
- .Net Socket (TcpClient/TcpListener)
- JS WebSocket ([RFC 6455 Spec](https://datatracker.ietf.org/doc/html/rfc6455))
- log4net
- xunit
- Microsoft.Extensions.Hosting

**Server SPEC**
- AWS LightSail
- CentOS 7
- 1 CPU
- 512MB Ram
- 20GB HDD

## 주요 기능
- 웹 소켓을 사용한 서버
- 웹 소켓 사양을 따른 Masking된 메시지 분석 기능
- 웹 소켓 사양을 따른 Non-Masking 메시지 발송 기능
- 접속한 Client와 동일한 Channel에 존재하는 모든 Client들의 행동 공유 기능
