import QtQuick 2.15
import QtQuick.Controls 2.15
import Chatbot 1.0

ApplicationWindow {
    visible: true
    width: 800
    height: 960
    title: "Azure AI Chatbot"

    AiConnector {
        id: ai
        onAnswerChanged: {
            historyModel.append({
                question: questionField.text,
                answer: ai.answer
            })
        }
    }

    ListModel {
        id: historyModel
    }

    Rectangle {
        anchors.fill: parent
        color: "#f5f5f5"

        Column {
            anchors.centerIn: parent
            spacing: 20
            width: parent.width * 0.95

            Text {
                text: "Ask question about the Help"
                font.pixelSize: 28
                font.bold: true
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
                text: "Press Enter"
                width: parent.width
                font.pixelSize: 18
                onClicked: ai.askQuestion(questionField.text)
            }

            Flickable {
                width: parent.width
                height: 300
                clip: true
                contentWidth: parent.width
                contentHeight: answerText.paintedHeight + 40
                flickableDirection: Flickable.VerticalFlick
                Rectangle {
                    width: parent.width
                    height: answerText.paintedHeight + 40
                    color: "#ffffff"
                    radius: 8
                    border.color: "#cccccc"

                    Text {
                        id: answerText
                        text: ai.answer
                        wrapMode: Text.Wrap
                        width: parent.width - 40
                        anchors.centerIn: parent
                        font.pixelSize: 16
                    }
                }
            }

            Text {
                text: "Previous Questions"
                font.pixelSize: 20
                font.bold: true
                padding: 10
            }

            ListView {
                width: parent.width
                height: 200
                model: historyModel
                clip: true
                spacing: 10

                delegate: Button {
                    width: parent.width
                    text: model.question
                    font.pixelSize: 16
                    onClicked: {
                        questionField.text = model.question
                        answerText.text = model.answer
                    }
                }
            }
        }
    }
}
