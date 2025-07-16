#include <QGuiApplication>
#include <QQmlApplicationEngine>
#include "AiConnector.h"
#include <QQuickStyle>
int main(int argc, char *argv[])
{
    QQuickStyle::setStyle("Fusion");  // or "Material", "Basic", etc.
    QGuiApplication app(argc, argv);
    qmlRegisterType<AiConnector>("Chatbot", 1, 0, "AiConnector");


    QCoreApplication::setOrganizationName("MyCompany");
    QCoreApplication::setOrganizationDomain("mycompany.com");
    QCoreApplication::setApplicationName("ChatbotApp");

    QQmlApplicationEngine engine;
    QObject::connect(
        &engine,
        &QQmlApplicationEngine::objectCreationFailed,
        &app,
        []() { QCoreApplication::exit(-1); },
        Qt::QueuedConnection);
    engine.loadFromModule("Chatbot", "Main");

    return app.exec();
}
