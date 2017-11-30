from distutils.core import setup

setup(
    name="quantom",
    version="0.1",
    scripts=['quantom.py'],

    # Project uses reStructuredText, so ensure that the docutils get
    # installed or upgraded on the target machine
    install_requires=['docutils>=0.3'],

    # metadata for upload to PyPI
    author="hardywu",
    author_email="hardy0wu@gmail.com",
    license="MIT",

    # other arguments here...
    entry_points={
        'console_scripts': [
            'quantom = quantom:main'
        ]
    }
)