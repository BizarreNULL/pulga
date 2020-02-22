#!/bin/bash

function usage() {
    echo "__________      .__                 ___ ___        .__                       "
    echo "\______   \__ __|  |   _________   /   |   \  ____ |  | ______   ___________ "
    echo " |     ___|  |  |  |  / ___\__  \ /    ~    _/ __ \|  | \____ \_/ __ \_  __ \\"
    echo " |    |   |  |  |  |_/ /_/  / __ \\\\    Y    \  ___/|  |_|  |_> \  ___/|  | \/"
    echo " |____|   |____/|____\___  (____  /\___|_  / \___  |____|   __/ \___  |__|   "
    echo "                    /_____/     \/       \/      \/     |__|        \/       "
    echo
    echo "Setup a certificate (.CRT format only) on remote."
    echo "Usage: $0 --serial=emnulator-5554 -c=certificate.crt"
    echo
    echo -e "\t-h --help        - Show this message, and exit."
    echo -e "\t-s --serial      - Android serial port to connect."
    echo -e "\t-c --certificate - Certificate path."
    echo
}

while [ "$1" != "" ]; do
    PARAM=$(echo $1 | awk -F= '{print $1}')
    VALUE=$(echo $1 | awk -F= '{print $2}')
    case $PARAM in
    -h | --help)
        usage
        exit
        ;;
    -c | --certificate)
        CERTIFICATE_PATH=$VALUE
        ;;
    -s | --serial)
        SERIAL_PORT=$VALUE
        ;;
    *)
        echo "[!] Unknown parameter ($PARAM)!"
        echo "try --help."
        ;;
    esac
    shift
done

if [ -z "$SERIAL_PORT" ]; then
    echo "[!] Serial port can't be empty!"
    echo "try --help."
    exit 111
fi

if [ -z "$CERTIFICATE_PATH" ]; then
    echo "[!] Serial port can't be empty!"
    echo "try --help."
    exit 111
fi

FILENAME=$(openssl x509 -in $CERTIFICATE_PATH -hash -noout)
echo $FILENAME
FILENAME=$FILENAME".0"

openssl x509 -in $CERTIFICATE_PATH >>$FILENAME
openssl x509 -in $CERTIFICATE_PATH -text -fingerprint -noout >>$FILENAME

adb -s $SERIAL_PORT shell "su 0 mount -o rw,remount /system"
adb -s $SERIAL_PORT push $FILENAME /sdcard
adb -s $SERIAL_PORT shell "su 0 mv /sdcard/$FILENAME /system/etc/security/cacerts"
rm $FILENAME
adb -s $SERIAL_PORT shell "su 0 chmod 644 /system/etc/security/cacerts/$FILENAME"
adb -s $SERIAL_PORT shell "su 0 mount -o ro,remount /system"

echo "Certificate installed as system trusted credential"
echo "Check if you able to see the new certificate listed in 'Settings > Security > Trusted Cert > Systems'"
