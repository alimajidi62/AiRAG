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
    color: "#eaeef3"
    property bool showHistory: true
    property bool isLoading: false
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
            isLoading = false
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
    RowLayout {
        anchors.fill: parent
        spacing: 0

        // Left Panel: Expandable History
        Rectangle {
            width: showHistory ? 280 : 50
            color: "#eeeefe"
            Layout.fillHeight: true
            border.color: "#5a555f"

            ColumnLayout {
                anchors.fill: parent
                spacing: 5
                //padding: 10

                Button {
                    text: showHistory ? "⮜" : "⮞"
                    font.pixelSize: 18
                    background: Rectangle {
                        color: "#34495e"
                        radius: 6
                    }
                    onClicked: showHistory = !showHistory
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
                    spacing: 5

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
                    }
                }
            }
        }

        // Right Panel: Chat UI
        Rectangle {
            Layout.fillWidth: true
            Layout.fillHeight: true
            color: "#f9f9fb"

            ColumnLayout {
                anchors.fill: parent
                anchors.margins: 30
                spacing: 20

                Text {
                    text: "💬 Azure AI Chatbot"
                    font.pixelSize: 32
                    font.bold: true
                    color: "#2c3e50"
                }

                TextField {
                    id: questionField
                    Layout.fillWidth: true
                    placeholderText: "Type your question..."
                    font.pixelSize: 18
                    padding: 10
                    background: Rectangle {
                        color: "#ffffff"
                        radius: 8
                        border.color: "#cccccc"
                    }
                    onAccepted: askButton.clicked()
                }

                Button {
                    id: askButton
                    text: "Ask"
                    Layout.fillWidth: true
                    font.pixelSize: 18
                    background: Rectangle {
                        color: "#3498db"
                        radius: 8
                    }
                    contentItem: Text {
                        text: qsTr("Ask")
                        color: "white"
                        font.pixelSize: 18
                        anchors.centerIn: parent
                    }
                    onClicked:
                    {
                        isLoading = true
                        ai.askQuestion(questionField.text)
                    }
                }
                BusyIndicator {
                    visible: isLoading
                    running: isLoading
                    anchors.horizontalCenter: parent.horizontalCenter
                    anchors.top: askButton.bottom
                    anchors.topMargin: 10
                    width: 40
                    height: 40
                }
                Rectangle {
                    Layout.fillWidth: true
                    Layout.preferredHeight: parent.height*.8
                    radius: 10
                    color: "#ffffff"
                    border.color: "#d0d0d0"

                    Flickable {
                        anchors.fill: parent
                        contentWidth: parent.width
                        contentHeight: answerText.paintedHeight + 40
                        clip: true
                        flickableDirection: Flickable.VerticalFlick

                        Text {
                            id: answerText
                            text: ai.answer
                            wrapMode: Text.Wrap
                            width: parent.width - 40
                            anchors.centerIn: parent
                            font.pixelSize: 16
                            color: "#2c3e50"
                        }
                    }
                }
            }
        }
    }
}
