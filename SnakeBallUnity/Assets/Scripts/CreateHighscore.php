<?php

$db = mysql_connect("localhost", "ramendev_jctwood", "M1n1flam3") 
	or die('Failed to connect: ' . mysql_error());

mysql_select_db('ramendev_snakeball') or die('Failed to access database');

$name = mysql_real_escape_string($_GET['name'], $db);
$score = mysql_real_escape_string($_GET['score'], $db);

$query = 
   "INSERT INTO highscores
	SET name = '$name', score = '$score', timestamp = CURRENT_TIMESTAMP;";

$result = mysql_query($query) or die('Query failed: ' . mysql_error());

echo mysql_insert_id($db) . " ";

?>