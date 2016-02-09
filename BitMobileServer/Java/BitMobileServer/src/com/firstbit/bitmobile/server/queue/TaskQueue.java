package com.firstbit.bitmobile.server.queue;

import java.util.UUID;

import com.google.appengine.api.taskqueue.Queue;
import com.google.appengine.api.taskqueue.QueueFactory;
import com.google.appengine.api.taskqueue.TaskOptions;

public class TaskQueue 
{
	public static void addUploadAdminTask(String solution, UUID sessionId, Boolean checkExisitng)
	{
        Queue queue = QueueFactory.getQueue("upload-admin");
        queue.add(TaskOptions.Builder.withUrl(String.format("/%s/admin/AsyncUpload", solution))
        		.param("sessionId", sessionId.toString())
        		.param("checkExisting", checkExisitng?"1":"0")        		
        	);
	}
	
	public static void addUploadMetadataTask(String solution, UUID sessionId)
	{
        Queue queue = QueueFactory.getQueue("upload-metadata");
        queue.add(TaskOptions.Builder.withUrl(String.format("/%s/admin/AsyncUploadMetadata", solution))
        		.param("sessionId", sessionId.toString())
        	);
	}
	
}
