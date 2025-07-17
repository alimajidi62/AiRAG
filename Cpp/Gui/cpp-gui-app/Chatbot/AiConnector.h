#ifndef AICONNECTOR_H
#define AICONNECTOR_H
#include <QObject>
#include <QString>
#include <QtConcurrent> // Add this include
class AiConnector : public QObject {
    Q_OBJECT
    Q_PROPERTY(QString answer READ answer NOTIFY answerChanged)
public:
    explicit AiConnector(QObject* parent = nullptr);

    Q_INVOKABLE void askQuestion(const QString& question);

    QString answer() const { return m_answer; }
    Q_INVOKABLE void uploadDocument(const QString& filePath);
signals:
    void answerChanged();

private:
    QString m_answer;
    void setAnswer(const QString& ans);
     void askQuestionImpl(const QString& question);
};

#endif // AICONNECTOR_H
