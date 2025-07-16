import QtQuick 2.15
import QtQuick.Controls 2.15
import QtQuick.Layouts 1.15
import Chatbot 1.0

ApplicationWindow {
    visible: true
    width: 1000
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

    property bool showHistory: true

    RowLayout {
        anchors.fill: parent
        spacing: 0

        // Left Panel: Expandable History
        Rectangle {
            width: showHistory ? 250 : 40
            color: "#eeeeee"
            Layout.fillHeight: true
            border.color: "#cccccc"

            ColumnLayout {
                anchors.fill: parent
                spacing: 5

                Button {
                    text: showHistory ? "<" : ">"
                    Layout.alignment: Qt.AlignLeft
                    onClicked: showHistory = !showHistory
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
            color: "#f5f5f5"

            ColumnLayout {
                anchors.fill: parent
                anchors.margins: 20
                spacing: 20

                Text {
                    text: "Ask question about the Help"
                    font.pixelSize: 28
                    font.bold: true
                }

                TextField {
                    id: questionField
                    Layout.fillWidth: true
                    placeholderText: "Type your question..."
                    font.pixelSize: 18
                    padding: 10
                    onAccepted: askButton.clicked()
                }

                Button {
                    id: askButton
                    text: "Press Enter"
                    Layout.fillWidth: true
                    font.pixelSize: 18
                    onClicked: ai.askQuestion(questionField.text)
                }

                Flickable {
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
                        }
                    }
                }
            }
        }
    }
}
