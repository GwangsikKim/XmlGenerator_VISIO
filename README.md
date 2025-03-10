# Visio P&ID CAD 도면 XML 추출 프로토타입

## 프로젝트 설명
이 프로그램은 **Visio**에서 P&ID (Piping and Instrumentation Diagram) CAD 도면의 정보를 **XML** 형식으로 추출하는 프로토타입입니다.
사용자는 Visio 파일을 입력으로 제공하고, 프로그램은 도면 내에서 도형의 속성, 텍스트 및 연결 관계를 추출하여 XML 파일로 저장합니다.

### 주요 기능
- **Visio 파일 열기**: .vsdx 형식의 Visio 파일을 열어 도면 정보를 읽습니다.
- **도형 정보 추출**: 각 도형의 이름, 유형, 텍스트 등의 정보를 추출합니다.
- **연결 관계 추적**: 도면 내에서 도형들 간의 연결 관계를 추적하여 XML에 포함시킵니다.
- **XML로 저장**: 추출된 정보를 XML 파일로 변환하여 저장합니다.

## 사용법

### 요구 사항
- **Microsoft Visio**: Visio 파일을 읽기 위한 프로그램입니다. Visio API를 사용하여 도면을 분석합니다.
- **.NET Framework**: C# 환경에서 개발되었습니다.

### 설치 방법
1. 이 프로젝트를 클론하거나 다운로드합니다.
    ```bash
    git clone https://github.com/yourusername/visio-pid-to-xml.git
    ```
2. Visual Studio 또는 다른 C# IDE에서 프로젝트를 열어 실행합니다.
3. `Microsoft.Office.Interop.Visio` 라이브러리를 참조에 추가하여 Visio API를 사용할 수 있도록 설정합니다.

### 실행 방법
1. 프로젝트를 빌드하고 실행합니다.
2. 실행 후, Visio 파일의 경로를 입력합니다.
3. 도면의 정보를 추출한 후 XML 파일로 저장됩니다.
