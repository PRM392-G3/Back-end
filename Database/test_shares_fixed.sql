-- Test script for shares functionality with correct column names
-- This script tests the shares table with lowercase column names

-- 1. Check shares table structure
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'shares' 
AND table_schema = 'public'
ORDER BY ordinal_position;

-- 2. Check existing shares data
SELECT 
    s.id,
    s.userid,
    s.postid,
    s.caption,
    s.ispublic,
    s.createdat
FROM public.shares s
ORDER BY s.createdat DESC;

-- 3. Check share counts by post
SELECT 
    s.postid,
    COUNT(*) as ShareCount,
    COUNT(CASE WHEN s.ispublic = true THEN 1 END) as PublicShareCount,
    COUNT(CASE WHEN s.ispublic = false THEN 1 END) as PrivateShareCount
FROM public.shares s
GROUP BY s.postid
ORDER BY s.postid;

-- 4. Check shares by user
SELECT * FROM public.shares WHERE userid = 1;

-- 5. Check shares by post
SELECT * FROM public.shares WHERE postid = 1;

-- 6. Check public shares
SELECT * FROM public.shares WHERE ispublic = true;

-- 7. Check private shares
SELECT * FROM public.shares WHERE ispublic = false;

-- 8. Test insert new share
INSERT INTO public.shares (userid, postid, caption, ispublic, createdat) VALUES
(4, 1, 'Test share from user 4', true, CURRENT_TIMESTAMP);

-- 9. Verify the new share
SELECT * FROM public.shares WHERE userid = 4;

-- 10. Clean up test data (optional)
-- DELETE FROM public.shares WHERE userid = 4;
