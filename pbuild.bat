:: $Id: pbuild.bat 7729 2020-01-24 09:42:56Z onuchin $
:: Author: Valeriy Onuchin   19.04.2014
::

call defines.bat
%msbuild%  .\TM.vs10.sln /t:Build /p:Configuration=release

pause

