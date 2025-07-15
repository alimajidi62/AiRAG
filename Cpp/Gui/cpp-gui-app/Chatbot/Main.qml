import QtQuick 2.15
import QtQuick.Controls 2.15
import Chatbot 1.0

ApplicationWindow {
    visible: true
    width: 400
    height: 300
    title: "Azure AI Chatbot"

    AiConnector {
        id: ai
    }

    Column {
        anchors.centerIn: parent
        spacing: 10

        TextField {
            id: questionField
            width: parent.width * 0.8
            placeholderText: "Type your question..."
            onAccepted: askButton.clicked()
        }

        Button {
            id: askButton
            text: "Ask"
            onClicked: {
                ai.askQuestion(questionField.text)
            }
        }

        TextArea {
            width: parent.width * 0.8
            height: 120
            readOnly: true
            text: ai.answer
            wrapMode: TextArea.Wrap
        }
    }
}
