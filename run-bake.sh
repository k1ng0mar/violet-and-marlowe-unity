#!/bin/bash
cd /home/ubuntu/violet-and-marlowe-unity
exec /home/ubuntu/.unity/bin/unity \
  -batchmode \
  -nographics \
  -executeMethod ScaleBakeRunner.Bake \
  -logFile /home/ubuntu/violet-and-marlowe-unity/bake-log.txt
