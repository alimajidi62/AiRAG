import QtQuick 2.15
import QtQuick.Controls 2.15
import QtQuick.Layouts 1.15
import Qt.labs.settings 1.0
import Chatbot 1.0

ApplicationWindow {
    visible: true
    width: 1000
    height: 960
    title: "Azure AI Chatbot"
    color: "#f0f4f8"

    Settings {
        id: settings
        property string savedHistoryJson: "[]"
    }

    AiConnector {
        id: ai
        onAnswerChanged: {
            let newEntry = {
                question: questionField.text,
                answer: ai.answer
            }
            historyModel.append(newEntry)
            answerText.text = ai.answer

            let historyArray = []
            for (let i = 0; i < historyModel.count; i++) {
                historyArray.push(historyModel.get(i))
            }
            settings.savedHistoryJson = JSON.stringify(historyArray)
        }
    }

    ListModel {
        id: historyModel
    }

    Component.onCompleted: {
        let saved = JSON.parse(settings.savedHistoryJson)
        for (let i = 0; i < saved.length; i++) {
            historyModel.append(saved[i])
        }
    }

    property bool showHistory: true

    RowLayout {
        anchors.fill: parent
        spacing: 0

        // Left Panel: History
        Rectangle {
            width: showHistory ? 260 : 50
            color: "#e3eaf0"
            Layout.fillHeight: true
            border.color: "#c0cbd4"
            radius: 8

            ColumnLayout {
                anchors.fill: parent
                spacing: 10
                //padding: 10

                Button {
                    text: showHistory ? "⮜" : "⮞"
                    font.pixelSize: 18
                    background: Rectangle {
                        color: "#34495e"
                        radius: 6
                    }
                    onClicked: showHistory = !showHistory
                    background: Rectangle {
                        color: "#d0dce7"
                        radius: 6
                    }
                }

                Button {
                    text: "🗑 Clear History"
                    visible: showHistory
                    Layout.alignment: Qt.AlignCenter
                    onClicked: {
                        historyModel.clear()
                        settings.savedHistoryJson = "[]"
                        answerText.text = ""
                        questionField.text = ""
                    }
                    background: Rectangle {
                        color: "#ffdddd"
                        radius: 6
                    }
                }

                ListView {
                    visible: showHistory
                    Layout.fillWidth: true
                    Layout.fillHeight: true
                    model: historyModel
                    clip: true
                    spacing: 6

                    delegate: Button {
                        width: parent.width
                        text: model.question
                        font.pixelSize: 14
                        background: Rectangle {
                            color: "#dddddd"
                            radius: 6
                        }
                        onClicked: {
                            questionField.text = model.question
                            answerText.text = model.answer
                        }
                        background: Rectangle {
                            color: "#ffffff"
                            border.color: "#c0cbd4"
                            radius: 6
                        }
                    }
                }
            }
        }

        // Right Panel: Chat UI
        Rectangle {
            Layout.fillWidth: true
            Layout.fillHeight: true
            color: "#ffffff"
            radius: 8

            ColumnLayout {
                anchors.fill: parent
                anchors.margins: 24
                spacing: 20

                Text {
                    text: "💬 Ask a question"
                    font.pixelSize: 28
                    font.bold: true
                    color: "#2c3e50"
                }

                TextField {
                    id: questionField
                    Layout.fillWidth: true
                    placeholderText: "Type your question..."
                    font.pixelSize: 18
                    padding: 12
                    background: Rectangle {
                        color: "#f0f4f8"
                        radius: 8
                        border.color: "#c0cbd4"
                    }
                    onAccepted: askButton.clicked()
                }

                Button {
                    id: askButton
                    text: "Ask"
                    Layout.fillWidth: true
                    font.pixelSize: 18
                    background: Rectangle {
                        color: "#0078d4"
                        radius: 8
                    }
                    contentItem: Text {
                        text: askButton.text
                        color: "white"
                        font.pixelSize: 18
                        anchors.centerIn: parent
                    }
                    onClicked: ai.askQuestion(questionField.text)
                }

                Rectangle {
                    Layout.fillWidth: true
                    Layout.preferredHeight: 300
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
                            color: "#333"
                        }
                    }
                }
            }
        }
    }
}
