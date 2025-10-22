-- Fix shares table to match existing database schema
-- This script updates the shares table to match the actual database structure

-- Drop existing shares table if it exists
DROP TABLE IF EXISTS public.shares CASCADE;

-- Create shares table with correct column names (lowercase)
CREATE TABLE public.shares (
  id SERIAL PRIMARY KEY,                           -- Khóa chính, tự tăng
  userid integer NOT NULL,                         -- Người chia sẻ bài viết
  postid integer NOT NULL,                         -- Bài viết được chia sẻ
  caption character varying(500),                  -- Nội dung chú thích khi chia sẻ (nếu có)
  ispublic boolean DEFAULT true,                   -- Có công khai không
  createdat timestamp without time zone DEFAULT CURRENT_TIMESTAMP -- Ngày tạo
);

-- Create indexes for better performance
CREATE INDEX idx_shares_user_id ON public.shares(userid);
CREATE INDEX idx_shares_post_id ON public.shares(postid);
CREATE INDEX idx_shares_created_at ON public.shares(createdat);
CREATE INDEX idx_shares_is_public ON public.shares(ispublic);

-- Optional: Add unique constraint to prevent duplicate shares
-- CREATE UNIQUE INDEX uq_user_post_share ON public.shares(userid, postid);

-- Insert some test data
INSERT INTO public.shares (userid, postid, caption, ispublic, createdat) VALUES
(1, 1, 'Bài viết này thật hay!', true, CURRENT_TIMESTAMP),
(2, 1, 'Tôi đồng ý với quan điểm này', true, CURRENT_TIMESTAMP),
(1, 2, 'Chia sẻ bài viết này cho bạn bè', false, CURRENT_TIMESTAMP),
(3, 1, 'Cảm ơn bạn đã chia sẻ', true, CURRENT_TIMESTAMP);

-- Verify the table structure
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'shares' 
AND table_schema = 'public'
ORDER BY ordinal_position;

-- Check the data
SELECT * FROM public.shares ORDER BY createdat DESC;
