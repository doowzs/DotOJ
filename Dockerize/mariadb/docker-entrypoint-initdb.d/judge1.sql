CREATE USER IF NOT EXISTS ''judge1''@''%'' IDENTIFIED BY ''judge1'';
CREATE DATABASE IF NOT EXISTS judge1;
GRANT ALL PRIVILEGES ON judge1.* TO ''judge1''@''%'';

CREATE USER IF NOT EXISTS ''hangfire''@''%'' IDENTIFIED BY ''hangfire'';
CREATE DATABASE IF NOT EXISTS hangfire;
GRANT ALL PRIVILEGES ON hangfire.* TO ''hangfire''@''%'';
