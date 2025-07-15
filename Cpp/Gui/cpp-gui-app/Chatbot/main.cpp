#include <QGuiApplication>
#include <QQmlApplicationEngine>
#include "AiConnector.h"
int main(int argc, char *argv[])
{
    QGuiApplication app(argc, argv);
    qmlRegisterType<AiConnector>("Chatbot", 1, 0, "AiConnector");
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
