<?php

$db = mysql_connect("localhost", "ramendev_jctwood", "M1n1flam3") 
	or die('Failed to connect: ' . mysql_error());

mysql_select_db('ramendev_snakeball') or die('Failed to access database');

$id = mysql_real_escape_string($_GET['id'], $db);

$query = 
   "SELECT  uo.*,
    (
	    SELECT COUNT(*)
	    FROM highscores ui
	    WHERE (ui.score, -ui.timestamp) >= (uo.score, -uo.timestamp)
    ) AS rank
	FROM highscores uo
	WHERE id = '$id';";

$result = mysql_query($query) or die('Query failed: ' . mysql_error());

echo $row['rank'];

?>