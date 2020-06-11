# Spectator-for-Unity
____
## Для чего он сделан.
1. Это дополнение было сделано для наблюдения, теперь  вы можете наблюдать за игроком, пока он играет в игру.
2. Но это, наверное, не очень интересно? Да!
3. Так что вы можете взаимодействовать с игроком:
   1. Путешествуйте с помощью управления!
   2. Может быть, вы хотите показать, где находитесь? Для этого нажмите англискую букву E.
- скины:
- ![глаз](https://media.githubusercontent.com/media/mentoster/Spectator-for-Unity/master/images/eye.png)
- ![svidetel](https://media.githubusercontent.com/media/mentoster/Spectator-for-Unity/master/images/svid.png)
- ![rodinaslishat](https://media.githubusercontent.com/media/mentoster/Spectator-for-Unity/master/images/Spy.png)
3. Может быть, вы хотели бы дать совет игроку или пошутить над ним?... Да! Вы тоже можете это сделать! Используйте англискую кнопку T для открытия чата!
____
# Информация для сервера
## Поддержка версий : 2018.4.18 f и выше.
## Установка:
1. Открыть [релизы](https://github.com/mentoster/Spectator-for-Unity/releases)
2. Загрузить пакет spectatorServer, откройте его, когда unity будет открыт.
- :warning: **для корректной работы в вашем проекте необходимо иметь пакет TMP!**
## Как использовать:
1. Перетащите prefab spectator на сцену.
2. Настройте, если это необходимо
3. Отлично! Все готово.
## Настройка
1. Чтобы изменить разрешение передаваемой картинки, перейдите в раздел **Spectator->Game View Encoder->resolution**
- вы также можете изменить качество картинки
- ![encoder](https://media.githubusercontent.com/media/mentoster/Spectator-for-Unity/master/images/videoEncoder.png)
2. Вы можете разблокировать вращение наблюдателя **Spectator - >Spectator Manager - >skin rotation**
- ![controller](https://media.githubusercontent.com/media/mentoster/Spectator-for-Unity/master/images/controller.png)
____
# Информация для клиента
## Вариант 1
1. Открыть [релизы](https://github.com/mentoster/Spectator-for-Unity/releases)
2. Загрузите compiled project и разархивируйте его, запустите.
3. Круто, у вас получилось!.
## Вариант 2 (получше, потому что вы можете изменить настройки, порты)
1. Открыть [релизы](https://github.com/mentoster/Spectator-for-Unity/releases) и загрузить spectatorClient
2. Создать проект unity ( версия 2019.3.10 f и выше).
3. Откройте  spectatorClient, когда проект открыт.
- :warning: **для корректной работы в вашем проекте необходимо иметь пакет TMP!**
### Как использовать:
1. Откройте сцену client->scenes->samplescene
2. Настройте свои параметры.
3. Отлично! Остается подождать, когда сервер запустится.

### Настройка
1. Чтобы изменить кнопку controlls, перейдите в ClientCamera->SpectManager->Spectator Manager.
-  ![controller](https://media.githubusercontent.com/media/mentoster/Spectator-for-Unity/master/images/clientSetting.png)
