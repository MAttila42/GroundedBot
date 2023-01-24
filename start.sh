#!/bin/bash
git pull
sudo docker compose down --volumes
sudo docker compose up -d --build
