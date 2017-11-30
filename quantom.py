""" quantom """
import argparse
import os
from dotenv import load_dotenv

PARSER = argparse.ArgumentParser(
    prog="quantom",
    description='Manager for Quantatative Finance Tools.')

PARSER.add_argument(
    'command',
    type=str,
    nargs="?",
    help='command argument')

PARSER.add_argument(
    '--config', '-c',
    help='configuration file')

def main():
    """ script function """
    args = PARSER.parse_args()
    if args.config:
        load_dotenv(args.config)
    if args.command == "console":
        from QUANTAXIS.QACmd import QA_cmd
        QA_cmd()

if __name__ == '__main__':
    main()
