import QtQuick 2.15
import QtQuick.Controls 2.15
import QtQuick.Layouts 1.15
import Qt.labs.settings 1.0
import QtQuick.Dialogs 6.2
import Chatbot 1.0

ApplicationWindow {
    visible: true
    width: 1000
    height: 960
    title: "Azure AI Chatbot"
    color: "#f4f6fb"
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

    ListModel { id: historyModel }
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
            width: showHistory ? 320 : 56
            color: "#e0e3ef"
            Layout.fillHeight: true
            border.color: "#bfc4d6"
            radius: 16

            ColumnLayout {
                anchors.fill: parent
                spacing: 12


                Button {
                    text: showHistory ? "⮜" : "⮞"
                    font.pixelSize: 22
                    background: Rectangle {
                        color: "#5a6cff"
                        radius: 10
                        border.color: "#9a9ccf"
                    }
                    contentItem: Text {
                        text: showHistory ? "⮜" : "⮞"
                        color: "white"
                        font.pixelSize: 22
                        anchors.centerIn: parent
                    }
                    hoverEnabled: true
                    onHoveredChanged: background.color = hovered ? "#3a4ccf" : "#5a6cff"
                    onClicked: showHistory = !showHistory
                }

                Button {
                    text: "🗑 Clear History"
                    visible: showHistory
                    Layout.alignment: Qt.AlignCenter
                    font.pixelSize: 16
                    background: Rectangle {
                        color: "#ff6b6b"
                        radius: 10
                        border.color: "#d43f3f"
                    }
                    contentItem: Text {
                        text: "🗑 Clear History"
                        color: "white"
                        font.pixelSize: 16
                        anchors.centerIn: parent
                    }
                    hoverEnabled: true
                    onHoveredChanged: background.color = hovered ? "#d43f3f" : "#ff6b6b"
                    onClicked: {
                        historyModel.clear()
                        settings.savedHistoryJson = "[]"
                        answerText.text = ""
                        questionField.text = ""
                    }
                }
                // ...existing code...

                Button {
                    id: openDocButton
                    text: "📄 Open Document"
                    font.pixelSize: 16
                    Layout.alignment: Qt.AlignCenter
                    background: Rectangle {
                        color: "#0078d4"
                        radius: 10
                        border.color: "#005a9e"
                    }
                    contentItem: Text {
                        text: openDocButton.text
                        color: "white"
                        font.pixelSize: 16
                        anchors.centerIn: parent
                    }
                    hoverEnabled: true
                    onHoveredChanged: background.color = hovered ? "#005a9e" : "#0078d4"
                    onClicked: fileDialog.open()
                }

                FileDialog {
                    id: fileDialog
                    title: "Select a document"
                    nameFilters: ["PDF Files (*.pdf)", "Text Files (*.txt)", "All Files (*)"]
                    onAccepted: {
                        // Pass the selected file path to C++
                        ai.uploadDocument(fileDialog.selectedFile)
                    }
                    // Optionally, set folder: StandardPaths.home
                }

                // ...existing code...

                ListView {
                    visible: showHistory
                    Layout.fillWidth: true
                    Layout.fillHeight: true
                    model: historyModel
                    clip: true
                    spacing: 8

                    delegate: Rectangle {
                        width: parent.width
                        height: 48
                        radius: 8
                        color: "#f7f8fa"
                        border.color: "#d0d3e2"

                        Button {
                            anchors.fill: parent
                            text: model.question
                            font.pixelSize: 15
                            background: Rectangle {
                                color: "transparent"
                                radius: 8
                            }
                            contentItem: Text {
                                text: model.question
                                color: "#2c3e50"
                                font.pixelSize: 15
                                anchors.verticalCenter: parent.verticalCenter
                                anchors.left: parent.left
                                anchors.leftMargin: 12
                            }
                            onClicked: {
                                questionField.text = model.question
                                answerText.text = model.answer
                            }
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
            radius: 18
            border.color: "#e0e3ef"

            ColumnLayout {
                anchors.fill: parent
                spacing: 28

                Text {
                    text: "💬 Azure AI Chatbot"
                    font.pixelSize: 38
                    font.bold: true
                    color: "#9a9ccf"
                    anchors.horizontalCenter: parent.horizontalCenter
                }

                Rectangle {
                    Layout.fillWidth: true
                    radius: 12
                    color: "#f4f6fb"
                    border.color: "#d0d3e2"
                    height: 56

                    TextField {
                        id: questionField
                        anchors.fill: parent
                        anchors.margins: 8
                        placeholderText: "Type your question..."
                        font.pixelSize: 18
                        background: Rectangle {
                            color: "transparent"
                        }
                        onAccepted: askButton.clicked()
                    }
                }

                Button {
                    id: askButton
                    text: "Ask"
                    Layout.fillWidth: true
                    font.pixelSize: 20
                    background: Rectangle {
                        color: "#9a9ccf"
                        radius: 12
                        border.color: "#3a4ccf"
                    }
                    contentItem: Text {
                        text: qsTr("Ask")
                        color: "white"
                        font.pixelSize: 20
                        anchors.centerIn: parent
                    }
                    hoverEnabled: true
                    onHoveredChanged: background.color = hovered ? "#3a4ccf" : "#5a6cff"
                    onClicked: {
                        isLoading = true
                        ai.askQuestion(questionField.text)
                    }
                }

                BusyIndicator {
                    visible: isLoading
                    running: isLoading
                    anchors.horizontalCenter: parent.horizontalCenter
                    anchors.top: askButton.bottom
                    anchors.topMargin: 18
                    width: 48
                    height: 48
                    palette.highlight: "#5a6cff"
                }

                Rectangle {
                    Layout.fillWidth: true
                    Layout.preferredHeight: parent.height * 0.7
                    radius: 14
                    color: "#f7f8fa"
                    border.color: "#d0d3e2"

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
                            font.pixelSize: 18
                            color: "#222a3a"
                        }
                    }
                }
            }
        }
    }
}
