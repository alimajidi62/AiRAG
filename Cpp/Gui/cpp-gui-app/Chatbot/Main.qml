import QtQuick 2.15
import QtQuick.Controls 2.15
import Chatbot 1.0

ApplicationWindow {
    visible: true
    width: 800
    height: 500
    title: "Azure AI Chatbot"

    AiConnector {
        id: ai
    }

    Rectangle {
        anchors.fill: parent
        color: "#f5f5f5"

        Column {
            anchors.centerIn: parent
            spacing: 20
            width: Math.min(parent.width * 0.8, 600)

            Text {
                text: "Ask Azure AI"
                font.pixelSize: 28
                font.bold: true
                anchors.horizontalCenter: parent.horizontalCenter
            }

            TextField {
                id: questionField
                width: parent.width
                placeholderText: "Type your question..."
                font.pixelSize: 18
                padding: 10
                onAccepted: askButton.clicked()
            }

            Button {
                id: askButton
                text: "Ask"
                width: parent.width
                font.pixelSize: 18
                onClicked: ai.askQuestion(questionField.text)
            }

            ScrollView {
                width: parent.width
                height: 220
                clip: true

                Rectangle {
                    width: parent.width
                    color: "#ffffff"
                    radius: 8
                    border.color: "#cccccc"
                    // Let the rectangle grow with the text, but not less than the ScrollView height
                    height: Math.max(answerText.implicitHeight + 20, parent.height)
                    clip: true

                    Text {
                        id: answerText
                        text: ai.answer
                        wrapMode: Text.Wrap
                        anchors.margins: 10
                        anchors.fill: parent
                        font.pixelSize: 16
                    }
                }
            }
        }
    }
}
