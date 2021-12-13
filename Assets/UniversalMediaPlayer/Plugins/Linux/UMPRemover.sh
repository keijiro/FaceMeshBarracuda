#!/bin/sh
 
cd "$(dirname "$0")"
sudo apt-get purge libavutil-dev libavcodec-dev libavformat-dev libswscale-dev libxcb-randr0-dev
sudo apt-get autoremove
