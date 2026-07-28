#!/usr/bin/env python3
import subprocess
import sys

# Run Unity bake directly, bypassing bash option parsing
result = subprocess.run([
    '/home/ubuntu/.unity/bin/unity',
    '-batchmode',
    '-nographics',
    '-executeMethod', 'ScaleBakeRunner.Bake',
    '-logFile', '/home/ubuntu/violet-and-marlowe-unity/bake-log.txt'
], cwd='/home/ubuntu/violet-and-marlowe-unity', capture_output=True, text=True)

print("STDOUT:", result.stdout)
print("STDERR:", result.stderr)
print("RETURN CODE:", result.returncode)
sys.exit(result.returncode)
